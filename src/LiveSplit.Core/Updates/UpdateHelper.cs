using LiveSplit.Options;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UpdateManager;

namespace LiveSplit.Updates;

public static class UpdateHelper
{
    // LiveSplit YS Stage 4.10.0: the legacy updater transport is disabled.
    // Main application updates move to the independent GitHub Releases channel.
    public const bool LegacyUpdateTransportEnabled = false;

    public static readonly Version Version = GetVersionFromGit();
    public static string UserAgent => GetUserAgent();

    public static readonly List<Type> AlreadyChecked = [];

    /// <summary>
    /// Compatibility entry point retained for components that still reference the old helper.
    /// Stage 4.10.0 intentionally performs no legacy network/update-manager work here.
    /// </summary>
    public static void Update(Form form, Action closeAction, params IUpdateable[] updateables)
    {
        return;
    }

    private static Version GetVersionFromGit()
    {
        try
        {
            return Version.Parse($"{Git.LastTag}.{Git.CommitsSinceLastTag}");
        }
        catch (Exception ex)
        {
            Log.Error(ex);
            return null;
        }
    }

    private static string GetUserAgent()
    {
        string versionString = (Version != null) ? Version.ToString() : "Unknown";
        return $"LiveSplit/{versionString}";
    }
}
