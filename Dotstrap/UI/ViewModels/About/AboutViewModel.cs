using System.Windows;

namespace Dotstrap.UI.ViewModels.About
{
    public class AboutViewModel : NotifyPropertyChangedViewModel
    {
        public string Version => string.Format(Strings.Menu_About_Version, App.Version);

        public BuildMetadataAttribute BuildMetadata => App.BuildMetadata;

        public string BuildTimestamp => BuildMetadata.Timestamp.ToFriendlyString();
        public string BuildCommitHashUrl => $"{App.ProjectRepositoryUrl}/commit/{BuildMetadata.CommitHash}";

        public string RepositoryUrl => App.ProjectRepositoryUrl;
        public string IssuesUrl => $"{App.ProjectRepositoryUrl}/issues";
        public Visibility IssuesVisibility => App.HasProjectRepository ? Visibility.Visible : Visibility.Collapsed;

        public Visibility BuildInformationVisibility => App.IsProductionBuild ? Visibility.Collapsed : Visibility.Visible;
        public Visibility BuildCommitVisibility => App.IsActionBuild && App.HasProjectRepository ? Visibility.Visible : Visibility.Collapsed;
    }
}
