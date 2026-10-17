namespace LiveSplit.View;

/// <summary>
/// Identity information for the Yhc_Studio maintained LiveSplit fork.
/// Keep fork and upstream versions separate so upstream merges do not
/// accidentally reuse or overwrite the fork's release numbering.
/// </summary>
internal static class ForkInfo
{
    public const string ProductName = "LiveSplit YS";
    public const string Version = "1.0.5";
    public const string Maintainer = "Yhc_Studio";
    public const string MaintainerUrl = "https://github.com/Yhc-Studio";
    public const string MaintainerDisplayUrl = "github.com/Yhc-Studio";
    public const string RepositoryUrl = "https://github.com/Yhc-Studio/LiveSplit";
    public const string UpdateChannel = "stable";
    public const string UpdateManifestUrl = "https://github.com/Yhc-Studio/LiveSplit/releases/latest/download/latest.json";

    // Upstream baseline used to create LiveSplit YS v1.0.5.
    public const string UpstreamBase = "1.8.37-61-gf1b471f9";
    public const string UpstreamRevisionUrl = "https://github.com/LiveSplit/LiveSplit/tree/f1b471f9";
}
