using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotstrap.UI.ViewModels.Settings;

namespace Dotstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for DotstrapPage.xaml
    /// </summary>
    public partial class DotstrapPage
    {
        public DotstrapPage()
        {
            DataContext = new DotstrapViewModel();
            InitializeComponent();
        }
    }
}
