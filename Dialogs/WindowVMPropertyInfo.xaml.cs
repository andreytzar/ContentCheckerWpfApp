using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ContentCheckerWpfApp.Dialogs
{
    /// <summary>
    /// Interaction logic for WindowVMPropertyInfo.xaml
    /// </summary>
    public partial class WindowVMPropertyInfo : Window
    {
        public WindowVMPropertyInfo()
        {
            InitializeComponent();
        }
        private void BTNOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; Close();
        }

        private void BTNCancell_Click(object sender, RoutedEventArgs e) => Close();
    }
}
