using System.Windows;

using Dotstrap.UI.ViewModels.Settings;

namespace Dotstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for OptimizationPage.xaml
    /// </summary>
    public partial class OptimizationPage
    {
        private bool _initialLoad = false;

        public OptimizationPage()
        {
            SetupViewModel();
            InitializeComponent();
        }

        private void SetupViewModel()
        {
            var viewModel = new OptimizationViewModel();

            viewModel.RequestPageReloadEvent += (_, _) => SetupViewModel();

            DataContext = viewModel;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // refresh datacontext on page load to pick up changes made on the fast flags pages

            if (!_initialLoad)
            {
                _initialLoad = true;
                return;
            }

            SetupViewModel();
        }
    }
}
