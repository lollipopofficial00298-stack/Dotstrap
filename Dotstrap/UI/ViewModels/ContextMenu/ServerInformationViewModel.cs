using System.Windows;
using System.Windows.Input;
using Dotstrap.Integrations;
using CommunityToolkit.Mvvm.Input;

namespace Dotstrap.UI.ViewModels.ContextMenu
{
    internal class ServerInformationViewModel : NotifyPropertyChangedViewModel
    {
        private readonly ActivityWatcher _activityWatcher;

        public string InstanceId => _activityWatcher.Data.JobId;

        public string ServerType => _activityWatcher.Data.ServerType.ToTranslatedString();

        public string ServerLocation { get; private set; } = Strings.Common_Loading;

        public Visibility ServerLocationVisibility => App.Settings.Prop.ShowServerDetails ? Visibility.Visible : Visibility.Collapsed;

        public string Ping { get; private set; } = Strings.Common_Loading;

        public Visibility PingVisibility => App.Settings.Prop.ShowServerDetails ? Visibility.Visible : Visibility.Collapsed;

        public ICommand CopyInstanceIdCommand => new RelayCommand(CopyInstanceId);

        public ServerInformationViewModel(Watcher watcher)
        {
            _activityWatcher = watcher.ActivityWatcher!;

            if (ServerLocationVisibility == Visibility.Visible)
            {
                QueryServerLocation();
                QueryPing();
            }
        }

        public async void QueryPing()
        {
            int? ping = await _activityWatcher.Data.QueryPing();

            Ping = ping is null ? Strings.Common_NotAvailable : $"{ping} ms";

            OnPropertyChanged(nameof(Ping));
        }

        public async void QueryServerLocation()
        {
            string? location = await _activityWatcher.Data.QueryServerLocation();

            if (String.IsNullOrEmpty(location))
                ServerLocation = Strings.Common_NotAvailable;
            else
                ServerLocation = location;

            OnPropertyChanged(nameof(ServerLocation));
        }

        private void CopyInstanceId() => Clipboard.SetDataObject(InstanceId);
    }
}
