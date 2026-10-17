using System;

using UpdateManager;

namespace LiveSplit.Updates;

/// <summary>
/// Compatibility stub for the old UpdateManager integration.
/// LiveSplit YS Stage 4.10.0 no longer uses the official livesplit.org
/// application/update-manager endpoints. Main application updates are handled
/// by the independent LiveSplit YS GitHub release channel instead.
/// </summary>
[Obsolete("LiveSplit YS uses YSUpdateService instead of the legacy UpdateManager channel.")]
public class LiveSplitUpdateable : IUpdateable
{
    public string UpdateName => "LiveSplit YS (legacy updater disabled)";

    public string XMLURL => string.Empty;

    public string UpdateURL => string.Empty;

    public Version Version => UpdateHelper.Version;
}
