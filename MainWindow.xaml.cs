using ContentCheckerWpfApp.Data;
using ContentCheckerWpfApp.Dialogs;
using ContentCheckerWpfApp.Models;
using ContentCheckerWpfApp.Models.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Linq;
using Page = ContentCheckerWpfApp.Models.DB.Page;
using System.Windows.Data;

namespace ContentCheckerWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<object> collection=new();
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
                        var type = link.MediaType = response.Content.Headers?.ContentType?.MediaType;
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
                var wi = new WindowInputText();
                wi.TXT.Text = wi.Title = "Result";
                wi.TXTInput.Text = result;
                wi.ShowDialog();
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
                    var titl = site.Pages.Where(x => x.MediaType?.Contains("text") == true).Where(x => x.Title == item).ToList();
                    if (titl.Count > 1) list.AddRange(titl);
                }
                dataGrid.ItemsSource = list.OrderBy(x => x.Title).ToList();
                OnLog(this, $"Finished");
            }
        }

        private async void MITestLinksFrom_Click(object sender, RoutedEventArgs e)
        {
            var w = new WindowInputText();
            w.TXT.Text = w.Title = "Paste list links from ClipBoard. Links separator - Enter (\\n)";
            if (w.ShowDialog() != true) return;
            var list = new List<string>(w.TXTInput.Text.Split('\n'));
            ObservableCollection<Link> links = new ObservableCollection<Link>();
            dataGrid.ItemsSource = links;
            OnLog(this, $"Starting scan {list.Count} links");
            using HttpClient client = new();
            int i = 1;
            foreach (var item in list)
            {
                var link = new Link() { Href = item, DateTested = DateTime.Now };
                try
                {
                    var uri = UriHelper.CreateUri(link.Href);
                    if (!uri.IsAbsoluteUri)
                        link.MediaType = "Bad LINK";
                    else
                    {
                        HttpResponseMessage response = await client.GetAsync(uri);
                        var type = link.MediaType = response.Content.Headers?.ContentType?.MediaType;
                        link.LinkStatus = (int)response.StatusCode;
                    }
                }
                catch (Exception ex)
                {
                    link.MediaType = "ERROR LINK";
                    OnLog(this, $"Exception on Link {link.Href}:\n{ex.Message}");
                }
                links.Add(link);
                OnLog(this, $"{i} from {list.Count} {link.MediaType} {link.LinkStatus} {link.Href}");
                i++;
            }
        }

        private async void MIScanPageFromLocalList_Click(object sender, RoutedEventArgs e)
        {
            scanner?.StopScan();
            var w = new WindowComboBoxSelect();
            using var context = new LocalContext();
            w.CMBSelect.ItemsSource = await context.Sites.ToListAsync();
            w.TXT.Text = w.Title = "Select site to continue scan";
            if (w.ShowDialog() != true) return;
            if (w.CMBSelect.SelectedItem is Site site)
            {
                var wi = new WindowInputText();
                wi.TXT.Text = wi.Title = "Paste list Local Paths from ClipBoard. Links separator - Enter (\\n)";
                if (wi.ShowDialog() != true) return;
                var list = new List<string>(wi.TXTInput.Text.Split('\n'));
                scanner = new(site.AbsoluteUri);
                scanner.LogDelegate += OnLog;
                OnLog(this, "Loading site pages and Link");
                await context.Entry(site).Collection(x => x.Pages).LoadAsync();
                await context.Entry(site).Collection(x => x.Links).LoadAsync();
                context.Attach(site);
                foreach (var item in list)
                {
                    string path = Regex.Replace(item, @"[\x00-\x1F\x7F]", string.Empty);
                    await scanner.ScanPage(context, site, path, continuescan: true);
                }
                OnLog(this, "Finish");
            }
        }
        private void MISaveData_Click(object sender, RoutedEventArgs e)
        {
            var diag = new SaveFileDialog()
            {
                Filter = "CSV files (*.csv)|*.csv|XML files (*.xml)|*.xml",
                DefaultExt = "csv",
                FileName = "data"
            };
            if (diag.ShowDialog() != true) return;
            string filePath = diag.FileName;
            string extension = System.IO.Path.GetExtension(filePath).ToLower();

            // Сохраняем файл в зависимости от расширения
            if (extension == ".csv")
            {
                SaveDataGridToCsv(dataGrid, filePath);
            }
            else if (extension == ".xml")
            {
                SaveDataGridToXml(dataGrid, filePath);
            }
            else
            {
                MessageBox.Show("Unsupported file format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void SaveDataGridToCsv(DataGrid dataGrid, string filePath)
        {
            try
            {
                if (dataGrid.ItemsSource == null) return;
                var sb = new StringBuilder();
                var headers = dataGrid.Columns.Select(column => column.Header.ToString());
                sb.AppendLine(string.Join(",", headers));

                var items = dataGrid.ItemsSource.Cast<object>();
                foreach (var item in items)
                {
                    var row = new List<string>();
                    foreach (var column in dataGrid.Columns)
                    {
                        if (column is DataGridBoundColumn boundColumn)
                        {
                            var binding = boundColumn.Binding as Binding;
                            var bindingPath = binding?.Path?.Path;
                            if (!string.IsNullOrEmpty(bindingPath))
                            {
                                var property = item.GetType().GetProperty(bindingPath);
                                var value = property?.GetValue(item, null)?.ToString() ?? string.Empty;
                                value = EscapeCsvValue(value);
                                row.Add(value);
                            }
                        }
                        else
                        {
                            row.Add(string.Empty);
                        }
                    }
                    sb.AppendLine(string.Join(",", row));
                }
                File.WriteAllText(filePath, sb.ToString());
            }
            catch (Exception ex) { var e = ex; }
        }
        private string EscapeCsvValue(string value)
        {
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                value = $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }
        public void SaveDataGridToXml(DataGrid dataGrid, string filePath)
        {
            try
            {
                if (dataGrid.ItemsSource == null) return;
                var root = new XElement("Rows");
                var items = dataGrid.ItemsSource.Cast<object>();
                foreach (var item in items)
                {
                    var rowElement = new XElement("Row");

                    foreach (var column in dataGrid.Columns)
                    {
                        if (column is DataGridBoundColumn boundColumn)
                        {
                            var binding = boundColumn.Binding as Binding;
                            var bindingPath = binding?.Path?.Path;

                            if (!string.IsNullOrEmpty(bindingPath))
                            {
                                var property = item.GetType().GetProperty(bindingPath);
                                var value = property?.GetValue(item, null)?.ToString() ?? string.Empty;
                                var columnName = column.Header.ToString();
                                var cellElement = new XElement(columnName, value);
                                rowElement.Add(cellElement);
                            }
                        }
                    }
                    root.Add(rowElement);
                }
                var xDocument = new XDocument(root);
                xDocument.Save(filePath);
            }
            catch (Exception ex) { var e = ex; }
        }

        private void dataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            collection = new();
            dataGrid.Items?.Clear();
            dataGrid.ItemsSource = collection;
        }
    }
}