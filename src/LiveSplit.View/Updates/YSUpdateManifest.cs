using LiveSplit.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LiveSplit.Updates;

/// <summary>
/// Manifest consumed by the LiveSplit YS release channel.
/// This intentionally does not reuse the legacy UpdateManager XML format.
/// </summary>
internal sealed class YSUpdateManifest
{
    public string VersionText { get; private set; }
    public string Channel { get; private set; }
    public string DownloadUrl { get; private set; }
    public string Sha256 { get; private set; }
    public IReadOnlyList<string> Notes { get; private set; } = Array.Empty<string>();

    public Version Version => ParseVersion(VersionText);

    public static bool TryParse(string json, out YSUpdateManifest manifest, out string error)
    {
        manifest = null;
        error = null;

        try
        {
            dynamic root = JSON.FromString(json);
            if (root == null)
            {
                error = "The update manifest is empty.";
                return false;
            }

            string version = Convert.ToString(root.version, CultureInfo.InvariantCulture)?.Trim();
            string channel = Convert.ToString(root.channel, CultureInfo.InvariantCulture)?.Trim();
            string download = Convert.ToString(root.download, CultureInfo.InvariantCulture)?.Trim();
            string sha256 = Convert.ToString(root.sha256, CultureInfo.InvariantCulture)?.Trim();

            if (ParseVersion(version) == null)
            {
                error = "The update manifest contains an invalid version.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "stable";
            }

            if (string.IsNullOrWhiteSpace(download)
                || !Uri.TryCreate(download, UriKind.Absolute, out Uri downloadUri)
                || (downloadUri.Scheme != Uri.UriSchemeHttps && downloadUri.Scheme != Uri.UriSchemeHttp))
            {
                error = "The update manifest contains an invalid download URL.";
                return false;
            }

            var notes = new List<string>();
            object rawNotes = root.notes;
            if (rawNotes is IEnumerable<object> noteItems)
            {
                notes.AddRange(noteItems
                    .Select(x => Convert.ToString(x, CultureInfo.InvariantCulture)?.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x)));
            }

            manifest = new YSUpdateManifest
            {
                VersionText = version,
                Channel = channel,
                DownloadUrl = download,
                Sha256 = sha256 ?? string.Empty,
                Notes = notes
            };
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public static Version ParseVersion(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string core = value.Trim();
        if (core.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            core = core.Substring(1);
        }

        int suffix = core.IndexOfAny(['-', '+']);
        if (suffix >= 0)
        {
            core = core.Substring(0, suffix);
        }

        return System.Version.TryParse(core, out Version version) ? version : null;
    }
}
