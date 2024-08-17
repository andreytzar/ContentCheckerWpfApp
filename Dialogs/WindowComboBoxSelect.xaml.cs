using System.Windows;

namespace ContentCheckerWpfApp.Dialogs
{
    /// <summary>
    /// Interaction logic for WindowComboBoxSelect.xaml
    /// </summary>
    public partial class WindowComboBoxSelect : Window
    {
        public WindowComboBoxSelect()
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
