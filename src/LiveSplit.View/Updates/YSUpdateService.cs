using LiveSplit.View;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.Updates;

internal enum YSUpdateCheckState
{
    UpdateAvailable,
    UpToDate,
    Error
}

internal sealed class YSUpdateCheckResult
{
    public YSUpdateCheckState State { get; set; }
    public YSUpdateManifest Manifest { get; set; }
    public string ErrorMessage { get; set; }
}

/// <summary>
/// Stage 4.10.0 check-only implementation for the LiveSplit YS GitHub release channel.
/// Download/install is deliberately kept out of the process until the updater executable
/// and atomic replacement flow are ready.
/// </summary>
internal static class YSUpdateService
{
    public static Task<YSUpdateCheckResult> CheckAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(ForkInfo.UpdateManifestUrl);
                request.Method = "GET";
                request.UserAgent = $"{ForkInfo.ProductName.Replace(' ', '-')}/{ForkInfo.Version}";
                request.Accept = "application/json";
                request.Timeout = 8000;
                request.ReadWriteTimeout = 8000;
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using var response = (HttpWebResponse)request.GetResponse();
                using Stream responseStream = response.GetResponseStream();
                using var reader = new StreamReader(responseStream, Encoding.UTF8, true);
                string json = reader.ReadToEnd();

                if (!YSUpdateManifest.TryParse(json, out YSUpdateManifest manifest, out string error))
                {
                    return new YSUpdateCheckResult
                    {
                        State = YSUpdateCheckState.Error,
                        ErrorMessage = error
                    };
                }

                Version currentVersion = YSUpdateManifest.ParseVersion(ForkInfo.Version);
                Version latestVersion = manifest.Version;
                bool newer = currentVersion != null && latestVersion != null && latestVersion > currentVersion;

                return new YSUpdateCheckResult
                {
                    State = newer ? YSUpdateCheckState.UpdateAvailable : YSUpdateCheckState.UpToDate,
                    Manifest = manifest
                };
            }
            catch (WebException ex)
            {
                string message = ex.Response is HttpWebResponse response
                    ? $"HTTP {(int)response.StatusCode} {response.StatusDescription}"
                    : ex.Message;

                return new YSUpdateCheckResult
                {
                    State = YSUpdateCheckState.Error,
                    ErrorMessage = message
                };
            }
            catch (Exception ex)
            {
                return new YSUpdateCheckResult
                {
                    State = YSUpdateCheckState.Error,
                    ErrorMessage = ex.Message
                };
            }
        });
    }
}
