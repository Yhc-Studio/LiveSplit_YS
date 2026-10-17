using LiveSplit.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;

namespace LiveSplit.Model;

public class AutoSplitterFactory
{
    public static AutoSplitterFactory Instance { get; protected set; }
    public IDictionary<string, AutoSplitter> AutoSplitters { get; private set; }

    public const string AutoSplittersXmlUrl = "https://raw.githubusercontent.com/LiveSplit/LiveSplit.AutoSplitters/master/LiveSplit.AutoSplitters.xml";
    public const string AutoSplittersXmlFile = "LiveSplit.AutoSplitters.xml";
    private const int DownloadTimeoutMs = 5000;

    private readonly object syncRoot = new();
    private bool cacheLoadAttempted;

    static AutoSplitterFactory()
    {
        try
        {
            Instance = new AutoSplitterFactory();
            // Startup must never depend on GitHub/network availability. Only load the
            // local catalog here; TimerForm refreshes it online after the UI is ready.
            Instance.InitFromCache();
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    protected AutoSplitterFactory()
    {
    }

    /// <summary>
    /// Backwards-compatible initialization entry point. It is intentionally cache-only.
    /// </summary>
    public void Init() => InitFromCache();

    public void InitFromCache()
    {
        lock (syncRoot)
        {
            if (cacheLoadAttempted)
            {
                return;
            }

            cacheLoadAttempted = true;
        }

        if (!File.Exists(AutoSplittersXmlFile))
        {
            return;
        }

        try
        {
            var document = new XmlDocument();
            document.Load(AutoSplittersXmlFile);
            IDictionary<string, AutoSplitter> parsed = ParseAutoSplitters(document);
            lock (syncRoot)
            {
                AutoSplitters = parsed;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    /// <summary>
    /// Refreshes the Auto Splitter catalog from GitHub. This method may block for up to
    /// the configured network timeout and must therefore be called from a worker thread.
    /// </summary>
    public bool UpdateFromWeb()
    {
        try
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            XmlDocument document = DownloadAutoSplittersFromWeb();
            if (document == null)
            {
                return false;
            }

            IDictionary<string, AutoSplitter> parsed = ParseAutoSplitters(document);
            lock (syncRoot)
            {
                AutoSplitters = parsed;
                cacheLoadAttempted = true;
            }

            SaveCache(document);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex);
            return false;
        }
    }

    private static IDictionary<string, AutoSplitter> ParseAutoSplitters(XmlDocument document)
    {
        XmlElement root = document?["AutoSplitters"];
        if (root == null)
        {
            return new Dictionary<string, AutoSplitter>();
        }

        return root.ChildNodes.OfType<XmlElement>()
            .Where(element => element != null)
            .Select(CreateFromXmlElement)
            .SelectMany(x => x.Games.Select(y => new KeyValuePair<string, AutoSplitter>(y, x)))
            .ToDictionary(x => x.Key, x => x.Value);
    }

    public static AutoSplitter CreateFromXmlElement(XmlElement element)
    {
        string typeElementText = element["Type"]?.InnerText;
        string scriptTypeElementText = element["ScriptType"]?.InnerText;

        AutoSplitterType? autoSplitterType = null;
        if (typeElementText == "Component")
        {
            autoSplitterType = AutoSplitterType.Component;
        }
        else if (typeElementText == "Script")
        {
            autoSplitterType = scriptTypeElementText == "AutoSplittingRuntime" ? AutoSplitterType.AutoSplittingRuntimeScript : AutoSplitterType.Script;
        }

        XmlElement runtimeElement = element["AutoSplittingRuntime"];
        AutoSplittingRuntimeInfo runtimeInfo = null;
        if (runtimeElement != null)
        {
            runtimeInfo = new AutoSplittingRuntimeInfo
            {
                URL = runtimeElement["URL"]?.InnerText,
                Description = runtimeElement["Description"]?.InnerText,
                Website = runtimeElement["Website"]?.InnerText
            };
        }

        return new AutoSplitter()
        {
            Description = element["Description"].InnerText,
            URLs = [.. element["URLs"].ChildNodes.OfType<XmlElement>().Select(x => x.InnerText)],
            Type = autoSplitterType.Value,
            Games = element["Games"].ChildNodes.OfType<XmlElement>().Select(x => (x.InnerText ?? "").ToLower()).ToList(),
            ShowInLayoutEditor = element["ShowInLayoutEditor"] != null,
            Website = element["Website"]?.InnerText,
            AutoSplittingRuntime = runtimeInfo
        };
    }

    public AutoSplitter Create(string game)
    {
        // Never perform an online request from a Create call. This keeps startup and
        // Game Name editing deterministic even when GitHub/DNS is unavailable.
        InitFromCache();

        if (string.IsNullOrEmpty(game))
        {
            return null;
        }

        string normalizedGame = game.ToLowerInvariant();
        lock (syncRoot)
        {
            if (AutoSplitters != null && AutoSplitters.TryGetValue(normalizedGame, out AutoSplitter splitter))
            {
                return splitter;
            }
        }

        return null;
    }

    protected XmlDocument DownloadAutoSplittersFromWeb()
    {
        var request = WebRequest.CreateHttp(AutoSplittersXmlUrl);
        request.Timeout = DownloadTimeoutMs;
        request.ReadWriteTimeout = DownloadTimeoutMs;
        request.UserAgent = "LiveSplit";

        using WebResponse response = request.GetResponse();
        using Stream stream = response.GetResponseStream();
        var document = new XmlDocument();
        document.Load(stream);
        return document;
    }

    private static void SaveCache(XmlDocument document)
    {
        try
        {
            string tempFile = AutoSplittersXmlFile + ".tmp";
            document.Save(tempFile);

            if (File.Exists(AutoSplittersXmlFile))
            {
                File.Delete(AutoSplittersXmlFile);
            }

            File.Move(tempFile, AutoSplittersXmlFile);
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }
}
