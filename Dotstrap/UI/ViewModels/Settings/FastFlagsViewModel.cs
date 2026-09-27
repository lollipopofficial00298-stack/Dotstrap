using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using Dotstrap.Enums.FlagPresets;

namespace Dotstrap.UI.ViewModels.Settings
{
    public class FastFlagsViewModel : NotifyPropertyChangedViewModel
    {
        private Dictionary<string, object>? _preResetFlags;

        public event EventHandler? RequestPageReloadEvent;
        
        public event EventHandler? OpenFlagEditorEvent;

        private void OpenFastFlagEditor() => OpenFlagEditorEvent?.Invoke(this, EventArgs.Empty);

        public ICommand OpenFastFlagEditorCommand => new RelayCommand(OpenFastFlagEditor);

        public Visibility CanShowFastFlagEditor => App.IsStudioInstalled ? Visibility.Visible : Visibility.Collapsed;

        public bool UseFastFlagManager
        {
            get => App.Settings.Prop.UseFastFlagManager;
            set => App.Settings.Prop.UseFastFlagManager = value;
        }

        // texture quality, msaa, flat sky and grass motion presets live on the optimization page (OptimizationViewModel)

        public bool FixDisplayScaling
        {
            get => App.FastFlags.GetPreset("Rendering.DisableScaling") == "True";
            set => App.FastFlags.SetPreset("Rendering.DisableScaling", value ? "True" : null);
        }

        public IReadOnlyDictionary<GraphicsAPI, string> GraphicsAPIs => FastFlagManager.GraphicsAPITargets;

        public GraphicsAPI SelectedGraphicsAPI
        {
            get => App.FastFlags.GetPresetEnum(GraphicsAPIs, "Rendering.API", "True");
            set
            {
                // roblox's vulkan renderer crashes on some gpus (seen on amd) when the window is minimized or resized
                if (value == GraphicsAPI.Vulkan && SelectedGraphicsAPI != GraphicsAPI.Vulkan)
                {
                    var result = Frontend.ShowMessageBox(Strings.Menu_FastFlags_Presets_GraphicsAPI_VulkanWarning, MessageBoxImage.Warning, MessageBoxButton.YesNo);

                    if (result != MessageBoxResult.Yes)
                    {
                        // the combobox has already changed its text, so put it back once this binding update is done
                        Application.Current.Dispatcher.BeginInvoke(() => OnPropertyChanged(nameof(SelectedGraphicsAPI)));
                        return;
                    }
                }

                if (value == GraphicsAPI.Default)
                    App.FastFlags.SetPreset("Rendering.API", null);
                else
                    App.FastFlags.SetPresetEnum("Rendering.API", GraphicsAPIs[value], "True");
            }
        }

        public bool ResetConfiguration
        {
            get => _preResetFlags is not null;

            set
            {
                if (value)
                {
                    _preResetFlags = new(App.FastFlags.Prop);
                    App.FastFlags.Prop.Clear();
                }
                else
                {
                    App.FastFlags.Prop = _preResetFlags!;
                    _preResetFlags = null;
                }

                RequestPageReloadEvent?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
