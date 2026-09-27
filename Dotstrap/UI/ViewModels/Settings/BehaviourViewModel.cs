using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Dotstrap.UI.ViewModels.Settings
{
    public class BehaviourViewModel : NotifyPropertyChangedViewModel
    {
        private void RepairProtocolHandlers()
        {
            const string LOG_IDENT = "BehaviourViewModel::RepairProtocolHandlers";

            try
            {
                WindowsRegistry.RegisterPlayer();

                if (App.IsStudioInstalled)
                    WindowsRegistry.RegisterStudio();

                Frontend.ShowMessageBox(Strings.Menu_Behaviour_RepairProtocol_Done, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                Frontend.ShowMessageBox($"{Strings.Menu_Behaviour_RepairProtocol_Failed}\n\n{ex.Message}", MessageBoxImage.Error);
            }
        }

        public ICommand RepairProtocolHandlersCommand => new RelayCommand(RepairProtocolHandlers);

        public bool ConfirmLaunches
        {
            get => App.Settings.Prop.ConfirmLaunches;
            set => App.Settings.Prop.ConfirmLaunches = value;
        }

        public bool BackgroundUpdates
        {
            get => App.Settings.Prop.BackgroundUpdatesEnabled;
            set => App.Settings.Prop.BackgroundUpdatesEnabled = value;
        }

        public bool IsRobloxInstallationMissing => !App.IsPlayerInstalled && !App.IsStudioInstalled;

        public bool ForceRobloxReinstallation
        {
            get => App.State.Prop.ForceReinstall || IsRobloxInstallationMissing;
            set => App.State.Prop.ForceReinstall = value;
        }
    }
}
