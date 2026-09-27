using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using Dotstrap.Enums.FlagPresets;

namespace Dotstrap.UI.ViewModels.Settings
{
    public class OptimizationViewModel : NotifyPropertyChangedViewModel
    {
        // presets that make up the optimization page, used by the reset button
        private static readonly string[] OptimizationPresets =
        {
            "Rendering.QualityLevel",
            "Rendering.TextureQuality",
            "Rendering.MSAA",
            "Rendering.FlatSky",
            "Rendering.Grass",
            "Rendering.ReducedGrassMotion",
            "Rendering.PauseVoxelizer",
            "Geometry.LowerDetail"
        };

        public event EventHandler? RequestPageReloadEvent;

        public Visibility FastFlagManagerDisabledVisibility => App.Settings.Prop.UseFastFlagManager ? Visibility.Collapsed : Visibility.Visible;

        public ICommand ApplyMaxPerformanceCommand => new RelayCommand(ApplyMaxPerformance);

        public ICommand ResetOptimizationCommand => new RelayCommand(ResetOptimization);

        private void ApplyMaxPerformance()
        {
            SelectedQualityLevel = RenderQualityLevel.Level1;
            SelectedTextureQuality = TextureQuality.Level0;
            SelectedMSAALevel = MSAAMode.x1;
            FlatSky = true;
            DisableGrass = true;
            ReducedGrassMotion = true;
            PauseVoxelizer = true;
            LowerDetail = true;

            RequestPageReloadEvent?.Invoke(this, EventArgs.Empty);
        }

        private void ResetOptimization()
        {
            foreach (string preset in OptimizationPresets)
                App.FastFlags.SetPreset(preset, null);

            RequestPageReloadEvent?.Invoke(this, EventArgs.Empty);
        }

        public IReadOnlyDictionary<RenderQualityLevel, string?> QualityLevels => FastFlagManager.RenderQualityLevels;

        public RenderQualityLevel SelectedQualityLevel
        {
            get => QualityLevels.FirstOrDefault(x => x.Value == App.FastFlags.GetPreset("Rendering.QualityLevel")).Key;
            set => App.FastFlags.SetPreset("Rendering.QualityLevel", QualityLevels[value]);
        }

        public IReadOnlyDictionary<TextureQuality, string?> TextureQualities => FastFlagManager.TextureQualityLevels;

        public TextureQuality SelectedTextureQuality
        {
            get => TextureQualities.Where(x => x.Value == App.FastFlags.GetPreset("Rendering.TextureQuality.Level")).FirstOrDefault().Key;
            set
            {
                if (value == TextureQuality.Default)
                {
                    App.FastFlags.SetPreset("Rendering.TextureQuality", null);
                }
                else
                {
                    App.FastFlags.SetPreset("Rendering.TextureQuality.OverrideEnabled", "True");
                    App.FastFlags.SetPreset("Rendering.TextureQuality.Level", TextureQualities[value]);
                }
            }
        }

        public IReadOnlyDictionary<MSAAMode, string?> MSAALevels => FastFlagManager.MSAAModes;

        public MSAAMode SelectedMSAALevel
        {
            get => MSAALevels.FirstOrDefault(x => x.Value == App.FastFlags.GetPreset("Rendering.MSAA")).Key;
            set => App.FastFlags.SetPreset("Rendering.MSAA", MSAALevels[value]);
        }

        public bool FlatSky
        {
            get => App.FastFlags.GetPreset("Rendering.FlatSky") == "True";
            set => App.FastFlags.SetPreset("Rendering.FlatSky", value ? "True" : null);
        }

        public bool DisableGrass
        {
            get => App.FastFlags.GetPreset("Rendering.Grass.MaxDistance") == "0";
            set => App.FastFlags.SetPreset("Rendering.Grass", value ? "0" : null);
        }

        public bool ReducedGrassMotion
        {
            get => App.FastFlags.GetPreset("Rendering.ReducedGrassMotion") == "100";
            set => App.FastFlags.SetPreset("Rendering.ReducedGrassMotion", value ? "100" : null);
        }

        public bool PauseVoxelizer
        {
            get => App.FastFlags.GetPreset("Rendering.PauseVoxelizer") == "True";
            set => App.FastFlags.SetPreset("Rendering.PauseVoxelizer", value ? "True" : null);
        }

        public bool LowerDetail
        {
            get => App.FastFlags.GetPreset("Geometry.LowerDetail.L0") == "0";
            set => App.FastFlags.SetPreset("Geometry.LowerDetail", value ? "0" : null);
        }

        // everything below goes through windows / roblox's settings file rather than fast flags (see SystemTweaks)

        public IReadOnlyCollection<FramerateCap> FramerateCaps { get; } = Enum.GetValues<FramerateCap>().Where(x => x != FramerateCap.Unlimited).ToList();

        public FramerateCap SelectedFramerateCap
        {
            get => App.Settings.Prop.FramerateCap == FramerateCap.Unlimited ? FramerateCap.Fps1000 : App.Settings.Prop.FramerateCap;
            set => App.Settings.Prop.FramerateCap = value;
        }

        public bool PreferHighPerformanceGpu
        {
            get => App.Settings.Prop.PreferHighPerformanceGpu;
            set => App.Settings.Prop.PreferHighPerformanceGpu = value;
        }

        public bool HighPerformancePowerPlan
        {
            get => App.Settings.Prop.HighPerformancePowerPlan;
            set => App.Settings.Prop.HighPerformancePowerPlan = value;
        }

        public ICommand ClearCacheCommand => new RelayCommand(ClearCache);

        public ICommand OpenScreenshotsFolderCommand => new RelayCommand(() => OpenFolder(SystemTweaks.RobloxScreenshotsFolder));

        public ICommand OpenRobloxLogsFolderCommand => new RelayCommand(() => OpenFolder(SystemTweaks.RobloxLogsFolder));

        private static void OpenFolder(string path)
        {
            // roblox only makes the screenshots folder once the first screenshot is taken
            Directory.CreateDirectory(path);
            Process.Start("explorer.exe", path);
        }

        private void ClearCache()
        {
            if (SystemTweaks.IsRobloxRunning())
            {
                Frontend.ShowMessageBox(Strings.Menu_Optimization_ClearCache_RobloxRunning, MessageBoxImage.Warning);
                return;
            }

            var result = Frontend.ShowMessageBox(Strings.Menu_Optimization_ClearCache_Confirm, MessageBoxImage.Question, MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            long freed = SystemTweaks.ClearRobloxCache();

            Frontend.ShowMessageBox(String.Format(Strings.Menu_Optimization_ClearCache_Done, $"{freed / 1024d / 1024d:N1}"), MessageBoxImage.Information);
        }
    }
}
