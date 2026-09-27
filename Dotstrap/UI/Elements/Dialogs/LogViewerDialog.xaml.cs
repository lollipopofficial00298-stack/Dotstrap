using System.Windows;

namespace Dotstrap.UI.Elements.Dialogs
{
    /// <summary>
    /// Interaction logic for LogViewerDialog.xaml
    /// </summary>
    public partial class LogViewerDialog
    {
        public LogViewerDialog()
        {
            InitializeComponent();

            LogTextBox.Text = App.Logger.AsDocument;
            LogTextBox.ScrollToEnd();

            CopyButton.Click += delegate
            {
                Clipboard.SetDataObject(App.Logger.AsDocument);
            };

            OpenFolderButton.Click += delegate
            {
                if (Directory.Exists(Paths.Logs))
                    Process.Start("explorer.exe", Paths.Logs);
            };

            CloseButton.Click += delegate
            {
                Close();
            };
        }
    }
}
