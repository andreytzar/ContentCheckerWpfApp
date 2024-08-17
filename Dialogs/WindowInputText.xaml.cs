using System.Windows;


namespace ContentCheckerWpfApp.Dialogs
{
    /// <summary>
    /// Interaction logic for WindowInputText.xaml
    /// </summary>
    public partial class WindowInputText : Window
    {
        public WindowInputText()
        {
            InitializeComponent();
        }

        private void BTNOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult=true; Close();
        }

        private void BTNCancell_Click(object sender, RoutedEventArgs e) => Close();
    }
}
