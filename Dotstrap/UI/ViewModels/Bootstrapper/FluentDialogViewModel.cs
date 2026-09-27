using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace Dotstrap.UI.ViewModels.Bootstrapper
{
    public class FluentDialogViewModel : BootstrapperDialogViewModel
    {
        // no OS backdrop - Mica/Aero tint using the Windows system accent color instead of
        // the Dotstrap brand colours, which is why the dialog used to look blue/purple
        public BackgroundType WindowBackdropType { get; set; } = BackgroundType.None;

        // diagonal gradient tinted with the user's chosen accent color
        public Brush BackgroundColourBrush { get; set; } = MakeGradient();

        // the window sizes itself to its content, so compact mode just shrinks the content
        public bool IsCompact => App.Settings.Prop.CompactBootstrapperDialog;

        public double IconSize => IsCompact ? 48 : 64;

        public Thickness IconMargin => IsCompact ? new Thickness(0, 8, 0, 8) : new Thickness(0, 16, 0, 12);

        public Thickness ContentMargin => IsCompact ? new Thickness(24, 8, 24, 16) : new Thickness(32, 16, 32, 20);

        public static Brush MakeGradient() => App.Settings.Prop.AccentColor.GetBackgroundBrush(App.Settings.Prop.Theme.GetFinal());

        [Obsolete("Do not use this! This is for the designer only.", true)]
        public FluentDialogViewModel() : base()
        { }

        public FluentDialogViewModel(IBootstrapperDialog dialog, bool aero) : base(dialog)
        {
        }
    }
}
