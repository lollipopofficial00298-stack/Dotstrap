using System.Media;
using System.Web;
using System.Windows;
using System.Windows.Interop;

using Windows.Win32;
using Windows.Win32.Foundation;

namespace Dotstrap.UI.Elements.Dialogs
{
    // hmm... do i use MVVM for this?
    // this is entirely static, so i think im fine without it, and this way is just so much more efficient

    /// <summary>
    /// Interaction logic for ExceptionDialog.xaml
    /// </summary>
    public partial class ExceptionDialog
    {
        const int MAX_GITHUB_URL_LENGTH = 8192;

        public ExceptionDialog(Exception exception)
        {
            InitializeComponent();
            AddException(exception);

            string diagnosticInfo =
                $"Dotstrap v{App.Version}\n" +
                $"OS: {Environment.OSVersion}\n" +
                $"{exception.GetType()}: {exception.Message}\n{exception.StackTrace}";

            CopyDiagnosticsButton.Click += delegate
            {
                Clipboard.SetDataObject(diagnosticInfo);
            };

            if (!App.Logger.Initialized)
                LocateLogFileButton.Content = Strings.Dialog_Exception_CopyLogContents;

            string repoUrl = App.ProjectRepositoryUrl;
            string wikiUrl = App.ProjectHelpLink;

            string title = HttpUtility.UrlEncode($"[BUG] {exception.GetType()}: {exception.Message}");
            string log = HttpUtility.UrlEncode(App.Logger.AsDocument);

            string issueUrl = $"{repoUrl}/issues/new?template=bug_report.yaml&title={title}&log={log}";

            if (issueUrl.Length > MAX_GITHUB_URL_LENGTH)
            {
                // url is way too long for github. remove the log parameter.
                issueUrl = $"{repoUrl}/issues/new?template=bug_report.yaml&title={title}";

                if (issueUrl.Length > MAX_GITHUB_URL_LENGTH)
                    issueUrl = $"{repoUrl}/issues/new?template=bug_report.yaml"; // bruh
            }

            string helpMessage = String.Format(Strings.Dialog_Exception_Info_2, wikiUrl, issueUrl);

            // no repo of our own to report to - Dotstrap bugs shouldn't go to the Bloxstrap tracker
            if (!App.HasProjectRepository)
            {
                helpMessage = String.Format(Strings.Dialog_Exception_Info_2_Alt, wikiUrl);
                ReportExceptionButton.Visibility = Visibility.Collapsed;
            }

            HelpMessageMDTextBlock.MarkdownText = helpMessage;
            VersionText.Text = String.Format(Strings.Dialog_Exception_Version, App.Version);

            ReportExceptionButton.Click += (_, _) => Utilities.ShellExecute(issueUrl);

            LocateLogFileButton.Click += delegate
            {
                if (App.Logger.Initialized && !String.IsNullOrEmpty(App.Logger.FileLocation))
                    Utilities.ShellExecute(App.Logger.FileLocation);
                else
                    Clipboard.SetDataObject(App.Logger.AsDocument);
            };

            CloseButton.Click += delegate
            {
                Close();
            };

            SystemSounds.Hand.Play();

            Loaded += delegate
            {
                IntPtr hWnd = new WindowInteropHelper(this).Handle;
                PInvoke.FlashWindow((HWND)hWnd, true);
            };
        }

        private void AddException(Exception exception, bool inner = false)
        {
            if (!inner)
                ErrorRichTextBox.Selection.Text = $"{exception.GetType()}: {exception.Message}";

            if (exception.InnerException is null)
                return;

            ErrorRichTextBox.Selection.Text += $"\n\n[Inner Exception]\n{exception.InnerException.GetType()}: {exception.InnerException.Message}";

            AddException(exception.InnerException, true);
        }
    }
}
