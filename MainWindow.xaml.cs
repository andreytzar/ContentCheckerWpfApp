using ContentCheckerWpfApp.Data;
using ContentCheckerWpfApp.Dialogs;
using ContentCheckerWpfApp.Models;
using ContentCheckerWpfApp.Models.DB;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;


namespace ContentCheckerWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        SiteScanner? scanner = null;
        private async void MIFullSiteScan_Click(object sender, RoutedEventArgs e)
        {
            var w = new WindowInputText();
            w.TXT.Text = w.Title = "Input site adress";
            if (w.ShowDialog() != true) return;
            string siteurl = w.TXTInput.Text;
            scanner?.StopScan();
            scanner = new SiteScanner(siteurl);
            scanner.LogDelegate += OnLog;
            await scanner?.LoadSite();

            scanner?.StartScan();
        }

        void OnLog(object sender, string message)
        {
            Application.Current.Dispatcher.Invoke(() => { SBStatus.Content = message; });
        }

        private void MIStopScan_Click(object sender, RoutedEventArgs e)
       => scanner?.StopScan();

        private async void MIContinueScan_Click(object sender, RoutedEventArgs e)
        {
            scanner?.StopScan();
            var w = new WindowComboBoxSelect();
            using var context = new LocalContext();
            w.CMBSelect.ItemsSource = await context.Sites.ToListAsync();
            w.TXT.Text = w.Title = "Select site to continue scan";
            if (w.ShowDialog() != true) return;
            if (w.CMBSelect.SelectedItem is Site site)
            {
                string url = await context.Pages.Where(x => x.SiteId == site.Id)
                    .Where(x => x.StatusCode == 200)
                    .Where(x => x.Scanned == null)
                    .Where(x=>!string.IsNullOrEmpty(x.AbsoluteUrl))
                    .Select(x => x.AbsoluteUrl).FirstOrDefaultAsync();
                if (string.IsNullOrEmpty(url)) url = site.CurrentPage;
                OnLog(this, $"Try continue scan {url}");
                await Task.Delay(1000);
                scanner = new(url);
                scanner.LogDelegate += OnLog;
                scanner.LoadSite();
                scanner.ContinueScan();
            }

        }

        private async void MILoadSites_Click(object sender, RoutedEventArgs e)
        {
            using var context = new LocalContext();
            dataGrid.ItemsSource = await context.Sites.ToListAsync();
        }

        private async void MILoadPages_Click(object sender, RoutedEventArgs e)
        {
            using var context = new LocalContext();
            dataGrid.ItemsSource = await context.Pages.ToListAsync();
        }

        private async void MILoadLinks_Click(object sender, RoutedEventArgs e)
        {
            using var context = new LocalContext();
            dataGrid.ItemsSource = await context.Links.ToListAsync();
        }

        private async void MIDeleteSite_Click(object sender, RoutedEventArgs e)
        {
            scanner?.StopScan();
            var w = new WindowComboBoxSelect();
            using var context = new LocalContext();
            w.CMBSelect.ItemsSource = await context.Sites.ToListAsync();
            w.TXT.Text = w.Title = "Select site to delete";
            if (w.ShowDialog() != true) return;
            if (w.CMBSelect.SelectedItem is Site site)
            {
                try
                {
                    OnLog(this, "Try Delete site....");
                    await context.Entry(site).Collection(x => x.Links).LoadAsync();
                    await context.Entry(site).Collection(x => x.Pages).LoadAsync();
                    context.RemoveRange(site.Links);
                    context.RemoveRange(site.Pages);
                    context.Remove(site);
                    await context.SaveChangesAsync();
                    OnLog(this, "Site deleted");
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }
    }
}