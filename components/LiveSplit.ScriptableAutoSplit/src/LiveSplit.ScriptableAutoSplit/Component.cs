using LiveSplit.ASL;
using LiveSplit.Model;
using LiveSplit.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components;

public class ASLComponent : LogicComponent
{
    public override string ComponentName => "Scriptable Auto Splitter";

    // public so other components (ASLVarViewer) can access
    public ASLScript Script { get; private set; }
    public event EventHandler ScriptChanged;

    private bool _do_reload;
    private string _old_script_path;
    private readonly Timer _update_timer;
    private readonly FileSystemWatcher _fs_watcher;
    private ComponentSettings _settings;
    private readonly LiveSplitState _state;

    // LiveSplit YS Stage 4.8: ASL parsing / Roslyn compilation is CPU-heavy and
    // used to run directly inside the WinForms timer tick. For layouts such as
    // Dawn.lsl this freezes the entire UI for 2-3 seconds immediately after the
    // layout appears. Keep all LiveSplit / settings interaction on the UI thread,
    // but move the pure file-read + ASLParser.Parse compilation step off-thread.
    private bool _disposed;
    private bool _load_in_progress;

    public ASLComponent(LiveSplitState state)
    {
        _state = state;
        _settings = new ComponentSettings();
        _fs_watcher = new FileSystemWatcher();

        async void handler<T>(object sender, T args)
        {
            await Task.Delay(200);
            if (!_disposed)
            {
                _do_reload = true;
            }
        }

        _fs_watcher.Changed += handler;
        _fs_watcher.Renamed += handler;

        // -try- to run a little faster than 60hz
        // note: Timer isn't very reliable and quite often takes ~30ms
        // we need to switch to threading
        _update_timer = new Timer() { Interval = 15 };
        _update_timer.Tick += (sender, args) => UpdateScript();
        _update_timer.Enabled = true;
    }

    public ASLComponent(LiveSplitState state, string script_path)
        : this(state)
    {
        _settings = new ComponentSettings(script_path);
    }

    public override void Dispose()
    {
        _disposed = true;
        _update_timer.Enabled = false;

        ScriptCleanup();
        try
        {
            ScriptChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }

        _fs_watcher?.Dispose();
        _update_timer?.Dispose();
    }

    public override Control GetSettingsControl(LayoutMode mode)
    {
        return _settings;
    }

    public override XmlNode GetSettings(XmlDocument document)
    {
        return _settings.GetSettings(document);
    }

    public override void SetSettings(XmlNode settings)
    {
        _settings.SetSettings(settings);
    }

    public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height,
        LayoutMode mode)
    { }

    private async void UpdateScript()
    {
        if (_disposed || _load_in_progress)
        {
            return;
        }

        // Disable timer, to wait for execution of this iteration to finish.
        // Unlike the upstream synchronous implementation, awaiting ASL parsing now
        // yields to the WinForms message loop, so the timer window remains usable.
        _update_timer.Enabled = false;
        try
        {
            // this is ugly, fix eventually!
            if (_settings.ScriptPath != _old_script_path || _do_reload)
            {
                _do_reload = false;
                string requestedScriptPath = _settings.ScriptPath;
                _old_script_path = requestedScriptPath;
                ScriptCleanup();

                if (string.IsNullOrEmpty(requestedScriptPath))
                {
                    // Only disable file watcher if script path changed to empty
                    // (otherwise detecting file changes may still be wanted)
                    _fs_watcher.EnableRaisingEvents = false;
                }
                else
                {
                    await LoadScriptAsync(requestedScriptPath);
                }

                if (_disposed)
                {
                    return;
                }

                ScriptChanged?.Invoke(this, EventArgs.Empty);
            }

            if (!_disposed && Script != null)
            {
                try
                {
                    Script.Update(_state);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
        finally
        {
            _load_in_progress = false;
            if (!_disposed)
            {
                _update_timer.Enabled = true;
            }
        }
    }

    private async Task LoadScriptAsync(string scriptPath)
    {
        Log.Info("[ASL] Loading new script asynchronously: " + scriptPath);

        _fs_watcher.Path = Path.GetDirectoryName(scriptPath);
        _fs_watcher.Filter = Path.GetFileName(scriptPath);
        _fs_watcher.EnableRaisingEvents = true;

        _load_in_progress = true;

        // ASLParser.Parse constructs ASLMethod instances. Those compile the ASL
        // blocks through Roslyn (CSharpCompilation.Emit), which is the expensive
        // 2-3 second operation observed with Dawn.lsl. This portion is pure parsing
        // / compilation and does not need the WinForms UI thread.
        ASLScript parsedScript = await Task.Run(() =>
        {
            string scriptText = File.ReadAllText(scriptPath);
            return ASLParser.Parse(scriptText);
        });

        // await resumes on the WinForms synchronization context because this method
        // starts from the WinForms Timer tick. Do not install a stale result if the
        // layout/component was disposed or the user selected another script while
        // compilation was in progress.
        if (_disposed
            || !string.Equals(scriptPath, _settings.ScriptPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        Script = parsedScript;
        Script.RefreshRateChanged += (sender, rate) => _update_timer.Interval = (int)Math.Round(1000 / rate);
        _update_timer.Interval = (int)Math.Round(1000 / Script.RefreshRate);
        Script.GameVersionChanged += (sender, version) => _settings.SetGameVersion(version);
        _settings.SetGameVersion(null);

        // Run startup only after compilation has completed and we are back on the
        // UI thread. ASL startup code is allowed to interact with LiveSplit state
        // and component settings, so keeping this portion on the UI thread avoids
        // changing the ASL compatibility contract.
        try
        {
            ASLSettings settings = Script.RunStartup(_state);
            _settings.SetASLSettings(settings);
        }
        catch (Exception ex)
        {
            // Script already created, but startup failed, so clean up again
            Log.Error(ex);
            ScriptCleanup();
        }
    }

    private void ScriptCleanup()
    {
        if (Script == null)
        {
            return;
        }

        try
        {
            Script.RunShutdown(_state);
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
        finally
        {
            _settings.SetGameVersion(null);
            _settings.ResetASLSettings();
            // Script should no longer be used, even in case of error
            // (which the ASL shutdown method may contain)
            Script = null;
        }
    }
}
