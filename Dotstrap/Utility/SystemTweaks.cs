using Microsoft.Win32;

namespace Dotstrap.Utility
{
    /// <summary>
    /// Optimizations done through Windows and Roblox's own settings files - none of these touch the Roblox process or client files.
    /// Every method logs and swallows its own failures, so a tweak going wrong never stops Roblox from launching.
    /// </summary>
    public static class SystemTweaks
    {
        private const string HighPerformancePowerScheme = "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c";

        private const string GpuPreferencesKey = @"Software\Microsoft\DirectX\UserGpuPreferences";
        private const string HighPerformanceGpuValue = "GpuPreference=2;";

        private static string RobloxLocalAppData => Path.Combine(Paths.LocalAppData, "Roblox");

        #region Framerate cap
        public static int? GetValue(this FramerateCap cap) => cap switch
        {
            FramerateCap.Fps300 => 300,
            FramerateCap.Fps360 => 360,
            FramerateCap.Fps480 => 480,
            // roblox rejects -1 (what studio stores for no cap) and drops to 60 fps, so the old unlimited option gets 1000 instead
            FramerateCap.Fps1000 or FramerateCap.Unlimited => 1000,
            _ => null
        };

        /// <summary>
        /// Writes the chosen cap into the player's GlobalBasicSettings file, which is where the in-game "Maximum Frame Rate" option is saved.
        /// </summary>
        public static void ApplyFramerateCap()
        {
            const string LOG_IDENT = "SystemTweaks::ApplyFramerateCap";

            int? cap = App.Settings.Prop.FramerateCap.GetValue();

            if (cap is null)
                return;

            try
            {
                // the number in the file name is a settings version, pick the newest one (ignoring studio's)
                var file = Directory.Exists(RobloxLocalAppData)
                    ? Directory.GetFiles(RobloxLocalAppData, "GlobalBasicSettings_*.xml")
                        .Where(x => !Path.GetFileNameWithoutExtension(x).EndsWith("_Studio", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(x => x.Length).ThenByDescending(x => x, StringComparer.OrdinalIgnoreCase)
                        .FirstOrDefault()
                    : null;

                if (file is null)
                {
                    App.Logger.WriteLine(LOG_IDENT, "GlobalBasicSettings not found (Roblox hasn't been launched yet?), skipping");
                    return;
                }

                string contents = File.ReadAllText(file);
                var regex = new Regex(@"(<int name=""FramerateCap"">)-?\d+(</int>)");

                if (!regex.IsMatch(contents))
                {
                    App.Logger.WriteLine(LOG_IDENT, $"No FramerateCap entry in {Path.GetFileName(file)}, skipping");
                    return;
                }

                string updated = regex.Replace(contents, $"${{1}}{cap}${{2}}", 1);

                if (updated != contents)
                    File.WriteAllText(file, updated);

                App.Logger.WriteLine(LOG_IDENT, $"Set FramerateCap to {cap} in {Path.GetFileName(file)}");
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }
        #endregion

        #region GPU preference
        /// <summary>
        /// Same as Settings > System > Display > Graphics > "High performance" in Windows, for the current Roblox executable.
        /// </summary>
        public static void ApplyGpuPreference(string executablePath)
        {
            const string LOG_IDENT = "SystemTweaks::ApplyGpuPreference";

            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(GpuPreferencesKey);

                // roblox's install folder changes with every update, so tidy up entries for versions that are gone
                foreach (string name in key.GetValueNames())
                {
                    if (name.StartsWith(Paths.Versions, StringComparison.OrdinalIgnoreCase) && !File.Exists(name))
                        key.DeleteValue(name, false);
                }

                if (App.Settings.Prop.PreferHighPerformanceGpu)
                {
                    key.SetValue(executablePath, HighPerformanceGpuValue);
                    App.Logger.WriteLine(LOG_IDENT, "Set high performance GPU preference");
                }
                else if (key.GetValue(executablePath) as string == HighPerformanceGpuValue)
                {
                    key.DeleteValue(executablePath, false);
                    App.Logger.WriteLine(LOG_IDENT, "Removed high performance GPU preference");
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }
        #endregion

        #region Power plan
        private static string? RunPowercfg(string args)
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "powercfg.exe",
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            });

            if (process is null)
                return null;

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);

            return process.ExitCode == 0 ? output : null;
        }

        /// <summary>
        /// Switches to the "High performance" power plan. Returns the plan that was active before, to hand back to <see cref="RestorePowerPlan"/>,
        /// or null if nothing was changed.
        /// </summary>
        public static string? EnableHighPerformancePowerPlan()
        {
            const string LOG_IDENT = "SystemTweaks::EnableHighPerformancePowerPlan";

            try
            {
                // powercfg's output is localized, so just pull the guid out of it
                var match = Regex.Match(RunPowercfg("/getactivescheme") ?? "", "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");

                if (!match.Success)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Could not read the active power plan");
                    return null;
                }

                string previous = match.Value;

                if (previous.Equals(HighPerformancePowerScheme, StringComparison.OrdinalIgnoreCase))
                    return null;

                if (RunPowercfg($"/setactive {HighPerformancePowerScheme}") is null)
                {
                    // some laptops (modern standby) only have the balanced plan
                    App.Logger.WriteLine(LOG_IDENT, "High performance power plan is not available on this PC");
                    return null;
                }

                App.Logger.WriteLine(LOG_IDENT, $"Switched power plan from {previous} to high performance");
                return previous;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                return null;
            }
        }

        public static void RestorePowerPlan(string previous)
        {
            const string LOG_IDENT = "SystemTweaks::RestorePowerPlan";

            try
            {
                RunPowercfg($"/setactive {previous}");
                App.Logger.WriteLine(LOG_IDENT, $"Restored power plan {previous}");
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }
        #endregion

        #region Cache
        public static bool IsRobloxRunning()
        {
            var processes = Process.GetProcessesByName(App.RobloxPlayerAppName)
                .Concat(Process.GetProcessesByName(App.RobloxStudioAppName))
                .ToArray();

            foreach (var process in processes)
                process.Dispose();

            return processes.Length > 0;
        }

        /// <summary>
        /// Deletes Roblox's logs, temporary files and downloaded asset cache (re-downloaded as needed).
        /// Settings, local storage and installed versions are left alone. Returns the number of bytes freed.
        /// </summary>
        public static long ClearRobloxCache()
        {
            const string LOG_IDENT = "SystemTweaks::ClearRobloxCache";

            var targets = new List<string>
            {
                Path.Combine(RobloxLocalAppData, "logs"),
                Path.Combine(RobloxLocalAppData, "rbx-storage"),
                Path.Combine(RobloxLocalAppData, "rbx-storage.db"),
                Path.Combine(RobloxLocalAppData, "rbx-storage.db-shm"),
                Path.Combine(RobloxLocalAppData, "rbx-storage.db-wal"),
                Path.Combine(Path.GetTempPath(), "Roblox")
            };

            long freed = 0;

            foreach (string target in targets)
            {
                var files = Directory.Exists(target)
                    ? Directory.EnumerateFiles(target, "*", SearchOption.AllDirectories)
                    : File.Exists(target) ? new[] { target } : Enumerable.Empty<string>();

                foreach (string file in files.ToList())
                {
                    try
                    {
                        long size = new FileInfo(file).Length;
                        File.Delete(file);
                        freed += size;
                    }
                    catch (Exception ex)
                    {
                        // a file in use by something else just gets skipped
                        App.Logger.WriteLine(LOG_IDENT, $"Could not delete {file} ({ex.Message})");
                    }
                }

                // tidy up folders left empty, but keep the top level folder itself
                if (Directory.Exists(target))
                {
                    foreach (string dir in Directory.EnumerateDirectories(target, "*", SearchOption.AllDirectories).OrderByDescending(x => x.Length).ToList())
                    {
                        try
                        {
                            if (!Directory.EnumerateFileSystemEntries(dir).Any())
                                Directory.Delete(dir);
                        }
                        catch { }
                    }
                }
            }

            App.Logger.WriteLine(LOG_IDENT, $"Freed {freed} bytes");
            return freed;
        }

        public static string RobloxLogsFolder => Path.Combine(RobloxLocalAppData, "logs");

        public static string RobloxScreenshotsFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Roblox");
        #endregion
    }
}
