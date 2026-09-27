using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace Dotstrap.UI.Elements.Base
{
    public abstract class WpfUiWindow : UiWindow
    {
        private readonly IThemeService _themeService = new ThemeService();

        /// <summary>
        /// Paints the window's root panel with the accent-coloured gradient instead of the plain theme background.
        /// The brush goes on the root panel rather than Window.Background, as WPF UI resets the latter whenever the backdrop is (re)applied.
        /// </summary>
        public bool UseAccentBackground { get; set; } = false;

        public WpfUiWindow()
        {
            ApplyTheme();
        }

        public void ApplyTheme()
        {
            const int customThemeIndex = 2; // index for CustomTheme merged dictionary

            _themeService.SetTheme(App.Settings.Prop.Theme.GetFinal() == Enums.Theme.Dark ? ThemeType.Dark : ThemeType.Light);

            // user-chosen accent color instead of the OS accent color (defaults to the Dotstrap green)
            var palette = App.Settings.Prop.AccentColor.GetPalette();
            Accent.Apply(palette.Base, palette.Primary, palette.Secondary, palette.Tertiary);

            // there doesn't seem to be a way to query the name for merged dictionaries
            var dict = new ResourceDictionary { Source = new Uri($"pack://application:,,,/UI/Style/{Enum.GetName(App.Settings.Prop.Theme.GetFinal())}.xaml") };
            Application.Current.Resources.MergedDictionaries[customThemeIndex] = dict;

            ApplyAccentBackground();

#if QA_BUILD
            this.BorderBrush = System.Windows.Media.Brushes.Red;
            this.BorderThickness = new Thickness(4);
#endif
        }

        private void ApplyAccentBackground()
        {
            if (UseAccentBackground && Content is Panel panel)
                panel.Background = App.Settings.Prop.AccentColor.GetBackgroundBrush(App.Settings.Prop.Theme.GetFinal());
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            ApplyAccentBackground();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            if (App.Settings.Prop.WPFSoftwareRender || App.LaunchSettings.NoGPUFlag.Active)
            {
                if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
                    hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
            }

            base.OnSourceInitialized(e);
        }
    }
}
