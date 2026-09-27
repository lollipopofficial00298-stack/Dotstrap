using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dotstrap.UI.ViewModels.Bootstrapper
{
    public class ClassicFluentDialogViewModel : BootstrapperDialogViewModel
    {
        public double FooterOpacity => Environment.OSVersion.Version.Build >= 22000 ? 0.4 : 1;

        // no OS Mica backdrop - it tints using the Windows system accent color rather than
        // the Dotstrap brand colours
        public Wpf.Ui.Appearance.BackgroundType WindowBackdropType { get; set; } = Wpf.Ui.Appearance.BackgroundType.None;

        public Brush BackgroundColourBrush { get; set; } = FluentDialogViewModel.MakeGradient();

        public ClassicFluentDialogViewModel(IBootstrapperDialog dialog) : base(dialog)
        {
        }
    }
}
