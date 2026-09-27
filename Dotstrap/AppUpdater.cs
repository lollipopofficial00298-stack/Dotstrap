using System.Windows;

namespace Dotstrap
{
    /// <summary>
    /// Offers Dotstrap updates when the menu or settings window is opened (the bootstrapper updates silently on its own when launching Roblox).
    /// </summary>
    public static class AppUpdater
    {
        // the launch menu can lead straight into the settings window in the same process - only ask once
        private static bool _prompted = false;

        /// <summary>
        /// Returns the latest release if it's newer than this build and it's safe to update right now.
        /// </summary>
        public static async Task<GithubRelease?> GetAvailableUpdate()
        {
            const string LOG_IDENT = "AppUpdater::GetAvailableUpdate";

            if (!App.Settings.Prop.CheckForUpdates || !App.HasProjectRepository)
                return null;

            // the update overwrites the installed exe, which can't happen while another instance (e.g. the watcher) runs from it
            if (Process.GetProcessesByName(App.ProjectName).Length > 1)
            {
                App.Logger.WriteLine(LOG_IDENT, "More than one Dotstrap instance running, not offering an update");
                return null;
            }

            var releaseInfo = await App.GetLatestRelease();

            if (releaseInfo is null || releaseInfo.Assets is null || !releaseInfo.Assets.Any())
                return null;

            if (Utilities.CompareVersions(App.Version, releaseInfo.TagName) != VersionComparison.LessThan)
            {
                App.Logger.WriteLine(LOG_IDENT, "No updates found");
                return null;
            }

            App.Logger.WriteLine(LOG_IDENT, $"Update available: {releaseInfo.TagName}");
            return releaseInfo;
        }

        /// <summary>
        /// Downloads the release's first asset (the exe) into the temp updates folder, returning where it was saved.
        /// </summary>
        public static async Task<string> DownloadRelease(GithubRelease releaseInfo)
        {
            const string LOG_IDENT = "AppUpdater::DownloadRelease";

            var asset = releaseInfo.Assets![0];

            // every release uploads its exe as plain "Dotstrap.exe", so name the download after the version -
            // otherwise an older download sitting in the folder gets picked up and "updates" to the same old version
            string downloadLocation = Path.Combine(Paths.TempUpdates, $"{App.ProjectName}-{releaseInfo.TagName}.exe");

            Directory.CreateDirectory(Paths.TempUpdates);
            CleanupUpdatesFolder();

            App.Logger.WriteLine(LOG_IDENT, $"Downloading {releaseInfo.TagName}...");

            // download to a temporary name first, so a cancelled download never leaves a broken exe behind
            string partialLocation = downloadLocation + ".part";

            var response = await App.HttpClient.GetAsync(asset.BrowserDownloadUrl);
            response.EnsureSuccessStatusCode();

            await using (var fileStream = new FileStream(partialLocation, FileMode.Create, FileAccess.Write))
                await response.Content.CopyToAsync(fileStream);

            File.Move(partialLocation, downloadLocation, true);

            return downloadLocation;
        }

        /// <summary>
        /// Removes leftover downloads from previous updates (anything that isn't the exe we're currently running from).
        /// </summary>
        private static void CleanupUpdatesFolder()
        {
            foreach (string file in Directory.GetFiles(Paths.TempUpdates))
            {
                if (file.Equals(Paths.Process, StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine("AppUpdater::CleanupUpdatesFolder", $"Could not delete {file} ({ex.Message})");
                }
            }
        }

        /// <summary>
        /// Checks for an update in the background and, if there is one, asks whether to install it.
        /// On yes, the new version is started with -upgrade (it installs itself, then carries on with <paramref name="relaunchArgs"/>) and this process exits.
        /// </summary>
        public static async void PromptForUpdate(params string[] relaunchArgs)
        {
            const string LOG_IDENT = "AppUpdater::PromptForUpdate";

            if (_prompted || App.LaunchSettings.TestModeFlag.Active)
                return;

            _prompted = true;

            GithubRelease? releaseInfo;

            try
            {
                releaseInfo = await GetAvailableUpdate();
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                return;
            }

            if (releaseInfo is null)
                return;

            string newVersion = releaseInfo.TagName.TrimStart('v');

            var result = Frontend.ShowMessageBox(
                String.Format(Strings.Dialog_UpdateAvailable, newVersion, App.Version),
                MessageBoxImage.Information,
                MessageBoxButton.YesNo,
                MessageBoxResult.Yes
            );

            if (result != MessageBoxResult.Yes)
            {
                App.Logger.WriteLine(LOG_IDENT, "User declined the update");
                return;
            }

            try
            {
                string downloadLocation = await DownloadRelease(releaseInfo);

                var startInfo = new ProcessStartInfo { FileName = downloadLocation };
                startInfo.ArgumentList.Add("-upgrade");

                foreach (string arg in relaunchArgs)
                    startInfo.ArgumentList.Add(arg);

                App.Settings.Save();

                // same handoff as the bootstrapper's updater: the new version waits on this lock until we've exited
                new InterProcessLock("AutoUpdater");

                App.Logger.WriteLine(LOG_IDENT, $"Starting {releaseInfo.TagName}...");
                Process.Start(startInfo);

                App.Terminate();
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to update");
                App.Logger.WriteException(LOG_IDENT, ex);

                Frontend.ShowMessageBox(String.Format(Strings.Bootstrapper_AutoUpdateFailed, newVersion), MessageBoxImage.Error);

                if (!String.IsNullOrEmpty(App.ProjectDownloadLink))
                    Utilities.ShellExecute(App.ProjectDownloadLink);
            }
        }
    }
}
