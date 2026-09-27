using System.Windows;

using Dotstrap.AppData;
using Dotstrap.Integrations;
using Dotstrap.Models;

namespace Dotstrap
{
    public class Watcher : IDisposable
    {
        private readonly InterProcessLock _lock = new("Watcher");

        private readonly WatcherData? _watcherData;
        
        private readonly NotifyIconWrapper? _notifyIcon;

        public readonly ActivityWatcher? ActivityWatcher;

        public readonly DiscordRichPresence? RichPresence;

        public readonly PlaytimeTracker? PlaytimeTracker;

        // power plan that was active before we switched to high performance, restored when roblox closes
        private string? _previousPowerPlan;

        private bool _integrationsDisposed = false;

        private readonly DateTime _startTime = DateTime.Now;

        public Watcher()
        {
            const string LOG_IDENT = "Watcher";

            if (!_lock.IsAcquired)
            {
                App.Logger.WriteLine(LOG_IDENT, "Watcher instance already exists");
                return;
            }

            string? watcherDataArg = App.LaunchSettings.WatcherFlag.Data;

            if (String.IsNullOrEmpty(watcherDataArg))
            {
#if DEBUG
                string path = new RobloxPlayerData().ExecutablePath;
                if (!File.Exists(path))
                    throw new ApplicationException("Roblox player is not been installed");

                using var gameClientProcess = Process.Start(path);

                _watcherData = new() { ProcessId = gameClientProcess.Id };
#else
                throw new Exception("Watcher data not specified");
#endif
            }
            else
            {
                _watcherData = JsonSerializer.Deserialize<WatcherData>(Encoding.UTF8.GetString(Convert.FromBase64String(watcherDataArg)));
            }

            if (_watcherData is null)
                throw new Exception("Watcher data is invalid");

            if (App.Settings.Prop.EnableActivityTracking)
            {
                ActivityWatcher = new(_watcherData.LogFile);

                if (App.Settings.Prop.UseDisableAppPatch)
                {
                    ActivityWatcher.OnAppClose += delegate
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Received desktop app exit, closing Roblox");
                        using var process = Process.GetProcessById(_watcherData.ProcessId);
                        process.CloseMainWindow();
                    };
                }

                if (App.Settings.Prop.UseDiscordRichPresence)
                    RichPresence = new(ActivityWatcher);

                if (App.Settings.Prop.TrackPlaytime)
                    PlaytimeTracker = new(ActivityWatcher);
            }

            _notifyIcon = new(this);
        }

        public void KillRobloxProcess() => CloseProcess(_watcherData!.ProcessId, true);

        public void CloseProcess(int pid, bool force = false)
        {
            const string LOG_IDENT = "Watcher::CloseProcess";

            try
            {
                using var process = Process.GetProcessById(pid);

                App.Logger.WriteLine(LOG_IDENT, $"Killing process '{process.ProcessName}' (pid={pid}, force={force})");

                if (process.HasExited)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"PID {pid} has already exited");
                    return;
                }

                if (force)
                    process.Kill();
                else
                    process.CloseMainWindow();
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"PID {pid} could not be closed");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        public async Task Run()
        {
            if (!_lock.IsAcquired || _watcherData is null)
                return;

            ActivityWatcher?.Start();

            if (App.Settings.Prop.HighPerformancePowerPlan)
                _previousPowerPlan = SystemTweaks.EnableHighPerformancePowerPlan();

            while (Utilities.GetProcessesSafe().Any(x => x.Id == _watcherData.ProcessId))
                await Task.Delay(1000);

            await CheckForCrash();

            if (_watcherData.AutoclosePids is not null)
            {
                foreach (int pid in _watcherData.AutoclosePids)
                    CloseProcess(pid);
            }

            if (App.LaunchSettings.TestModeFlag.Active)
                Process.Start(Paths.Process, "-settings -testmode");

            // only one watcher runs at a time, so with multi-instance launching other clients can still be open -
            // keep the power plan until they've all closed, with nothing left showing for the client we were watching
            if (_previousPowerPlan is not null && RobloxSingletonHolder.IsRobloxPlayerRunning())
            {
                App.Logger.WriteLine("Watcher::Run", "Other Roblox clients are still open, waiting for them to close before restoring the power plan");

                DisposeIntegrations();

                while (RobloxSingletonHolder.IsRobloxPlayerRunning())
                    await Task.Delay(3000);
            }

            RestorePowerPlan();
        }

        /// <summary>
        /// Lets the user know if Roblox crashed rather than being closed, going by the minidump Roblox's crash handler leaves behind.
        /// </summary>
        private async Task CheckForCrash()
        {
            const string LOG_IDENT = "Watcher::CheckForCrash";

            string reportsFolder = Path.Combine(Paths.LocalAppData, "Roblox", "logs", "crashes", "reports");

            // the crash handler can finish writing the dump a moment after the client is gone
            for (int attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    // with multi-instance launching, only count dumps from around now, not another client that crashed earlier on
                    DateTime since = DateTime.Now - TimeSpan.FromSeconds(30);

                    bool crashed = Directory.Exists(reportsFolder) && new DirectoryInfo(reportsFolder).EnumerateFiles("*.dmp")
                        .Any(x => x.LastWriteTime >= _startTime && x.LastWriteTime >= since);

                    if (crashed)
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Found a crash dump, Roblox crashed");

                        string message = Strings.Watcher_RobloxCrashed;

                        if (App.FastFlags.GetPreset("Rendering.API.Vulkan") == "True")
                            message += "\n\n" + Strings.Watcher_RobloxCrashed_Vulkan;

                        Frontend.ShowMessageBox(message, MessageBoxImage.Warning);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException(LOG_IDENT, ex);
                    return;
                }

                await Task.Delay(1000);
            }
        }

        private void RestorePowerPlan()
        {
            if (_previousPowerPlan is null)
                return;

            SystemTweaks.RestorePowerPlan(_previousPowerPlan);
            _previousPowerPlan = null;
        }

        private void DisposeIntegrations()
        {
            if (_integrationsDisposed)
                return;

            _integrationsDisposed = true;

            _notifyIcon?.Dispose();
            RichPresence?.Dispose();
            PlaytimeTracker?.Dispose();
        }

        public void Dispose()
        {
            App.Logger.WriteLine("Watcher::Dispose", "Disposing Watcher");

            // in case the watcher is closed early (e.g. from the tray menu) while roblox is still open
            RestorePowerPlan();

            DisposeIntegrations();

            GC.SuppressFinalize(this);
        }
    }
}
