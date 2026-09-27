namespace Dotstrap.UI.ViewModels.Installer
{
    public class WelcomeViewModel : NotifyPropertyChangedViewModel
    {
        // formatting is done here instead of in xaml, it's just a bit easier
        // the "official download sources" line is only shown once Dotstrap has its own repository to point at
        public string MainText => App.HasProjectRepository
            ? String.Format(
                Strings.Installer_Welcome_MainText,
                $"[github.com/{App.ProjectRepository}]({App.ProjectRepositoryUrl})",
                $"[{Strings.Installer_Welcome_ReleasesPage}]({App.ProjectDownloadLink})"
            )
            : Strings.Installer_Welcome_MainTextNoSources;

        public string DownloadLink => App.ProjectDownloadLink;

        public string VersionNotice { get; private set; } = "";

        public bool CanContinue { get; set; } = false;

        public event EventHandler? CanContinueEvent;

        // called by codebehind on page load
        public async void DoChecks()
        {
            var releaseInfo = await App.GetLatestRelease();

            if (releaseInfo is not null)
            {
                if (Utilities.CompareVersions(App.Version, releaseInfo.TagName) == VersionComparison.LessThan)
                {
                    VersionNotice = String.Format(Strings.Installer_Welcome_UpdateNotice, App.Version, releaseInfo.TagName.Replace("v", ""));
                    OnPropertyChanged(nameof(VersionNotice));
                }
            }

            CanContinue = true;
            OnPropertyChanged(nameof(CanContinue));

            CanContinueEvent?.Invoke(this, new EventArgs());
        }
    }
}
