using LiveSplit.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace LiveSplit.UI.Components;

public class ComponentManager
{
    public const string PATH_COMPONENTS = "Components\\";

    private static readonly object FactorySync = new();
    private static bool allFactoriesDiscovered;

    public static string BasePath { get; set; }

    // Stage 4.4: Keep the factory maps available from process start. Layout restore
    // can now lazy-load only the DLLs actually referenced by the active .lsl,
    // instead of scanning every DLL in Components on the UI thread.
    public static IDictionary<string, IComponentFactory> ComponentFactories { get; protected set; }
        = new ConcurrentDictionary<string, IComponentFactory>(StringComparer.OrdinalIgnoreCase);

    public static IDictionary<string, IRaceProviderFactory> RaceProviderFactories { get; set; }
        = new ConcurrentDictionary<string, IRaceProviderFactory>(StringComparer.OrdinalIgnoreCase);

    public static bool AllFactoriesDiscovered
    {
        get
        {
            lock (FactorySync)
            {
                return allFactoriesDiscovered;
            }
        }
    }

    public static ILayoutComponent LoadLayoutComponent(string path, LiveSplitState state)
    {
        IComponent component;

        if (string.IsNullOrEmpty(path))
        {
            component = new SeparatorComponent();
        }
        else
        {
            IComponentFactory factory = GetOrLoadComponentFactory(path);
            if (factory == null)
            {
                return null;
            }

            component = factory.Create(state);
        }

        return new LayoutComponent(path, component);
    }

    /// <summary>
    /// Gets a component factory from the cache, or loads only the requested DLL.
    /// This is the fast path used by startup layout restore and Auto Splitters.
    /// </summary>
    public static IComponentFactory GetOrLoadComponentFactory(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        EnsureFactoryCollections();
        if (ComponentFactories.TryGetValue(fileName, out IComponentFactory cached))
        {
            return cached;
        }

        lock (FactorySync)
        {
            if (ComponentFactories.TryGetValue(fileName, out cached))
            {
                return cached;
            }

            string fullPath = Path.GetFullPath(Path.Combine(BasePath ?? "", PATH_COMPONENTS, fileName));
            if (!File.Exists(fullPath))
            {
                return null;
            }

            IComponentFactory factory = LoadFactory<IComponentFactory>(fullPath);
            if (factory != null)
            {
                ComponentFactories[fileName] = factory;
            }

            return factory;
        }
    }

    /// <summary>
    /// Preloads only the component factories referenced by the prepared layout.
    /// Factory construction is deliberately separated from component construction:
    /// actual IComponent.Create(state) still happens on the UI thread.
    /// </summary>
    public static void PreloadComponentFactories(IEnumerable<string> fileNames, CancellationToken token = default)
    {
        if (fileNames == null)
        {
            return;
        }

        foreach (string fileName in fileNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            token.ThrowIfCancellationRequested();

            try
            {
                GetOrLoadComponentFactory(fileName);
            }
            catch
            {
                // Preloading is an optimization only. If a third-party factory is
                // not safe to initialize here, LoadLayoutComponent will retry on
                // the UI thread when the layout is actually activated.
            }
        }
    }

    public static void RegisterComponentFactory(string fileName, IComponentFactory factory)
    {
        if (string.IsNullOrWhiteSpace(fileName) || factory == null)
        {
            return;
        }

        EnsureFactoryCollections();
        lock (FactorySync)
        {
            ComponentFactories[fileName] = factory;
        }
    }

    /// <summary>
    /// Discovers every external component/race-provider factory exactly once per
    /// process. Stage 4.4 runs this only after the restored layout is interactive.
    /// </summary>
    public static void EnsureAllFactoriesLoaded(CancellationToken token = default)
    {
        if (AllFactoriesDiscovered)
        {
            return;
        }

        lock (FactorySync)
        {
            if (allFactoriesDiscovered)
            {
                return;
            }

            EnsureFactoryCollections();
            string path = Path.GetFullPath(Path.Combine(BasePath ?? "", PATH_COMPONENTS));
            if (!Directory.Exists(path))
            {
                allFactoriesDiscovered = true;
                return;
            }

            foreach (string file in Directory.EnumerateFiles(path, "*.dll"))
            {
                token.ThrowIfCancellationRequested();

                string fileName = Path.GetFileName(file);

                // If this DLL was already loaded lazily for the active layout,
                // do not construct its component factory a second time. It can
                // still theoretically be a race provider, so only skip the full
                // load when one of the maps already knows it.
                if (ComponentFactories.ContainsKey(fileName) || RaceProviderFactories.ContainsKey(fileName))
                {
                    continue;
                }

                object factory = LoadFactoryObject(file);
                if (factory is IComponentFactory componentFactory)
                {
                    ComponentFactories[fileName] = componentFactory;
                }

                if (factory is IRaceProviderFactory raceProviderFactory)
                {
                    RaceProviderFactories[fileName] = raceProviderFactory;
                }
            }

            allFactoriesDiscovered = true;
        }
    }

    public static IDictionary<string, T> LoadAllFactories<T>()
    {
        EnsureAllFactoriesLoaded();

        if (typeof(T) == typeof(IComponentFactory))
        {
            return ComponentFactories
                .Where(x => x.Value is T)
                .ToDictionary(x => x.Key, x => (T)(object)x.Value, StringComparer.OrdinalIgnoreCase);
        }

        if (typeof(T) == typeof(IRaceProviderFactory))
        {
            return RaceProviderFactories
                .Where(x => x.Value is T)
                .ToDictionary(x => x.Key, x => (T)(object)x.Value, StringComparer.OrdinalIgnoreCase);
        }

        var result = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        string path = Path.GetFullPath(Path.Combine(BasePath ?? "", PATH_COMPONENTS));
        if (!Directory.Exists(path))
        {
            return result;
        }

        foreach (string file in Directory.EnumerateFiles(path, "*.dll"))
        {
            T factory = LoadFactory<T>(file);
            if (factory != null)
            {
                result[Path.GetFileName(file)] = factory;
            }
        }

        return result;
    }

    public static T LoadFactory<T>(string path)
    {
        object factory = LoadFactoryObject(path);
        return factory is T typedFactory ? typedFactory : default;
    }

    private static object LoadFactoryObject(string path)
    {
        try
        {
            var attr = (ComponentFactoryAttribute)Attribute.GetCustomAttribute(
                Assembly.UnsafeLoadFrom(path),
                typeof(ComponentFactoryAttribute));

            return attr?
                .ComponentFactoryClassType
                .GetConstructor([])
                ?.Invoke(null);
        }
        catch
        {
            return null;
        }
    }

    private static void EnsureFactoryCollections()
    {
        ComponentFactories ??= new ConcurrentDictionary<string, IComponentFactory>(StringComparer.OrdinalIgnoreCase);
        RaceProviderFactories ??= new ConcurrentDictionary<string, IRaceProviderFactory>(StringComparer.OrdinalIgnoreCase);
    }
}
