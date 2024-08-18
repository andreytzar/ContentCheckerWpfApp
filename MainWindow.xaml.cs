using Azure;
using ContentCheckerWpfApp.Data;
using ContentCheckerWpfApp.Dialogs;
using ContentCheckerWpfApp.Models;
using ContentCheckerWpfApp.Models.DB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Page = ContentCheckerWpfApp.Models.DB.Page;

namespace ContentCheckerWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<object> collection;
        public MainWindow()
        {
            InitializeComponent();
            collection = new();
            dataGrid.ItemsSource = collection;
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
                    .Where(x => !string.IsNullOrEmpty(x.AbsoluteUrl))
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


        private async void MIScanEmptyTitle_Click(object sender, RoutedEventArgs e)
        {
            scanner?.StopScan();
            var w = new WindowComboBoxSelect();
            using var context = new LocalContext();
            w.CMBSelect.ItemsSource = await context.Sites.ToListAsync();
            w.TXT.Text = w.Title = "Select site to continue scan";
            if (w.ShowDialog() != true) return;
            if (w.CMBSelect.SelectedItem is Site site)
            {
                OnLog(this, $"Try continue scan {site.AbsoluteUri}");
                await Task.Delay(1000);
                scanner = new(site.AbsoluteUri);
                scanner.LogDelegate += OnLog;
                await scanner.LoadSite(site.Id);
                await scanner.ScanEmptyTitle();
            }
        }

        private async void MIFiltering_Click(object sender, RoutedEventArgs e)
        {
            var w = new WindowComboBoxSelect();
            using var context = new LocalContext();
            w.CMBSelect.ItemsSource = VMTable.VMTables;
            w.TXT.Text = w.Title = "Select table to filter";
            if (w.ShowDialog() != true) return;
            if (w.CMBSelect.SelectedItem is VMTable table)
            {
                var propinfos = PropertyHelper.GetNonCollectionProperties(table.Type);
                var vmprops = VMPropertyInfo.GetListVMPropInfo(propinfos);
                var wp = new WindowVMPropertyInfo();
                wp.Title = wp.TXT.Text = "Check and filter";
                wp.List.ItemsSource = vmprops;
                if (wp.ShowDialog() != true) return;
                IQueryable<object> objects = null;
                if (table.Type == typeof(Site)) objects = context.Sites.AsQueryable();
                if (table.Type == typeof(Link)) objects = context.Links.AsQueryable();
                if (table.Type == typeof(Page)) objects = context.Pages.AsQueryable();
                if (objects == null) return;
                var filteredData = VMPropertyInfo.ApplyFilters(objects, vmprops);
                dataGrid.ItemsSource = await filteredData.ToListAsync();
            }
        }

        private void dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        => e.Row.Header = (e.Row.GetIndex() + 1).ToString();


        private async void MITestLinks_Click(object sender, RoutedEventArgs e)
        {
            scanner?.StopScan();
            var w = new WindowComboBoxSelect();
            using var context = new LocalContext();
            w.CMBSelect.ItemsSource = await context.Sites.ToListAsync();
            w.TXT.Text = w.Title = "Select site to continue scan";
            if (w.ShowDialog() != true) return;
            if (w.CMBSelect.SelectedItem is Site site)
            {
                OnLog(this, "Loading Links");
                await context.Entry(site).Collection(x => x.Links).LoadAsync();
                OnLog(this, "Start scan Links");
                using HttpClient client = new();
                int i = 1;
                foreach (var link in site.Links)
                {
                    try
                    {
                        var uri = UriHelper.CreateUri(link.Href);
                        if (!uri.IsAbsoluteUri)
                        {
                            uri = UriHelper.GetAbsoluteUri(site.AbsoluteUri, link.Href);
                        }

                        HttpResponseMessage response = await client.GetAsync(uri);
                        var type = response.Content.Headers?.ContentType?.MediaType;
                        link.LinkStatus = (int)response.StatusCode;
                        OnLog(this, $"{i} from {site.Links.Count} {type} {link.LinkStatus} {uri}");
                        i++;
                    }
                    catch (Exception ex)
                    {
                        OnLog(this, $"Exception on Link {link.Href}:\n{ex.Message}");
                        link.LinkStatus = -1;
                    }
                    link.DateTested = DateTime.Now;
                    context.Update(link);
                }
                await context.SaveChangesAsync();
                var liststat = site.Links.Select(x => x.LinkStatus).Distinct().ToList();
                string result = "";
                foreach (var item in liststat)
                {
                    var count = site.Links.Where(x => x.LinkStatus == item).Count();
                    result = $"{result}Status {item}={count} links;\n";
                }
                OnLog(this, $"Scan Links finished\n{result}");
            }
        }

        private async void MIDoubleTitles_Click(object sender, RoutedEventArgs e)
        {
            scanner?.StopScan();
            var w = new WindowComboBoxSelect();
            using var context = new LocalContext();
            w.CMBSelect.ItemsSource = await context.Sites.ToListAsync();
            w.TXT.Text = w.Title = "Select site to continue scan";
            if (w.ShowDialog() != true) return;
            if (w.CMBSelect.SelectedItem is Site site)
            {
                OnLog(this, $"Anaise pages");
                await context.Entry(site).Collection(x => x.Pages).LoadAsync();
                List<Page> list = new();
                var titles = site.Pages.Select(x => x.Title).Distinct().ToList();
                foreach (var item in titles)
                {
                    var titl=site.Pages.Where(x=>x.MediaType?.Contains("text")==true).Where(x=>x.Title==item).ToList();
                    if (titl.Count>1) list.AddRange(titl);
                }
                dataGrid.ItemsSource=list.OrderBy(x=>x.Title).ToList();
                OnLog(this, $"Finished");
            }
        }
    }
}