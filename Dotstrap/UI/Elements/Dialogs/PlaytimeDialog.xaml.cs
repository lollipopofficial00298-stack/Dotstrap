using System.Windows;

namespace Dotstrap.UI.Elements.Dialogs
{
    /// <summary>
    /// Interaction logic for PlaytimeDialog.xaml
    /// </summary>
    public partial class PlaytimeDialog
    {
        private class Row
        {
            public string Name = "";
            public string TotalTimeDisplay = "";
            public int SessionCount;
        }

        public PlaytimeDialog()
        {
            InitializeComponent();

            LoadData();

            ClearButton.Click += delegate
            {
                var result = Frontend.ShowMessageBox(Strings.Dialog_Playtime_ClearConfirm, MessageBoxImage.Warning, MessageBoxButton.YesNo);

                if (result != MessageBoxResult.Yes)
                    return;

                App.PlaytimeState.Prop.Games.Clear();
                App.PlaytimeState.Save();

                LoadData();
            };

            CloseButton.Click += delegate
            {
                Close();
            };
        }

        private void LoadData()
        {
            var rows = App.PlaytimeState.Prop.Games.Values
                .OrderByDescending(x => x.TotalSeconds)
                .Select(x => new Row
                {
                    Name = String.IsNullOrEmpty(x.Name) ? Strings.Common_NotAvailable : x.Name,
                    TotalTimeDisplay = TimeSpan.FromSeconds(x.TotalSeconds).ToString(@"hh\:mm\:ss"),
                    SessionCount = x.SessionCount
                })
                .ToList();

            GamesListView.ItemsSource = rows;

            EmptyText.Visibility = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            GamesListView.Visibility = rows.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
