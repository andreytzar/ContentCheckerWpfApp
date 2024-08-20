using ContentCheckerWpfApp.Data;
using ContentCheckerWpfApp.Models.DB;
using System.Net.Http;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System;




namespace ContentCheckerWpfApp
{
    public delegate void LogDelegate(object sender, string message);
    public class SiteScanner
    {
        public Site? CurrentSite { get; private set; }
        public LogDelegate? LogDelegate { get; set; }
        public string CurrentUrl { get; private set; } = string.Empty;
        public bool Stop { get; set; } = false;
        public SiteScanner(string url)
        {
            CurrentUrl = url;
        }

        public async Task LoadSite()
        {
            OnLog($"{DateTime.Now} Loading site {CurrentUrl}");
            var uri = UriHelper.GetSiteUri(CurrentUrl);
            CurrentSite = await GetSite(uri);
        }
        public async Task LoadSite(int id)
        {
            OnLog($"{DateTime.Now} Loading site id={id}");
            using var context = new LocalContext();
            CurrentSite = await context.Sites
                .FirstOrDefaultAsync(x => x.Id == id);
            if (CurrentSite == null) return;
            OnLog($"Loading Pages {CurrentSite.AbsoluteUri}....");
            await context.Entry(CurrentSite).Collection(x => x.Pages).LoadAsync();
            OnLog($"Loading Links {CurrentSite.AbsoluteUri}.....");
            await context.Entry(CurrentSite).Collection(x => x.Links).LoadAsync();
            OnLog($"Loaded");

        }
        public async Task StartScan()
        {
            Stop = false;
            OnLog($"{DateTime.Now} Start Scan {CurrentUrl}");
            if (CurrentSite == null) await LoadSite();
            using var context = new LocalContext();
            context.Attach(CurrentSite);
            await ScanPage(context, CurrentSite, CurrentUrl);
            OnLog($"{DateTime.Now} Full Scan {CurrentUrl}");

        }

        public async Task<Site> GetSite(string url)
        {
            url = UriHelper.GetSiteUri(url);
            OnLog($"{DateTime.Now} Loading Site {url}");
            using var context = new LocalContext();
            return await context.GetSite(url);
        }

        public async Task ContinueScan()
        {
            Stop = false;
            OnLog($"{DateTime.Now} Continue Site {CurrentUrl}");
            CurrentSite = await GetSite(CurrentUrl);
            using var context = new LocalContext();
            context.Attach(CurrentSite);
            await ScanPage(context, CurrentSite, CurrentSite.CurrentPage, continuescan: true);
            OnLog($"{DateTime.Now} Full Scan");
        }

        public void StopScan()
        {
            OnLog($"{DateTime.Now} Stopping scan Site {CurrentUrl}");
            Stop = true;
        }
        public async Task ScanEmptyTitle()
        {
            var emptylinks = CurrentSite.Pages.Where(x => string.IsNullOrEmpty(x.Title)).ToList();
            using var context = new LocalContext();
            foreach (var page in emptylinks)
            {
                await ScanPage(context, CurrentSite, page.PathAndQuary, continuescan: true);
            }
            OnLog("Empty Title Rescanned!");
        }
        public async Task<Page?> ScanPage(LocalContext context, Site site, string path, bool rescan = false, bool scanlinks = true, bool continuescan = false)
        {
            if (Stop) return null;
            OnLog($"{DateTime.Now} Scan Page {path}");
            try
            {

                var uri = UriHelper.CreateUri(path);
                if (uri == null) return null;
                if (uri.IsAbsoluteUri)
                {
                    if (!UriHelper.IsUrlBelongsToSite(path, site.Host))
                    {
                        OnLog($"{DateTime.Now} Page {path} Not Belongs To Site");
                        return null;
                    }
                    uri = UriHelper.CreateUri(uri.PathAndQuery);
                }
                if (!continuescan && !rescan)
                {
                    if (site.Pages.FirstOrDefault(x => string.Equals(x.PathAndQuary, uri.OriginalString, StringComparison.Ordinal) && x.Scanned != null) != null)
                    {
                        OnLog($"{DateTime.Now} Allready scanned {path}");
                        return null;
                    }
                }

                if (continuescan) continuescan = false;
                var uripage = UriHelper.GetAbsoluteUri(site.AbsoluteUri, uri.OriginalString);
                var page = site.Pages.FirstOrDefault(x => string.Equals(x.PathAndQuary, uri.OriginalString, StringComparison.Ordinal));
                if (page == null) page = await context.AddPage(site, uri.OriginalString);
                if (page == null)
                {
                    OnLog($"{DateTime.Now} Error path {path} ");
                    return null;
                }
                page.AbsoluteUrl = uripage.AbsoluteUri;
                site.CurrentPage = page.AbsoluteUrl;
                page.MediaType = "Try test page";
                context.Update(page);
                context.Update(site);
                await context.SaveChangesAsync();
                using HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(page.AbsoluteUrl);

                page.StatusCode = (int)response.StatusCode;
                await context.SaveChangesAsync();
                if (response.IsSuccessStatusCode)
                {
                    string pageContent = await response.Content.ReadAsStringAsync();
                    page.MediaType = response.Content?.Headers?.ContentType?.MediaType;

                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(pageContent);
                    page.Title = HtmlHelper.GetTitle(doc);
                    page.Description = HtmlHelper.GetMetaDescription(doc);
                    page.OgSiteName = HtmlHelper.GetOgSiteName(doc);
                    page.CanonicalLink = HtmlHelper.GetCanonicalLink(doc);
                    page.OgUrl = HtmlHelper.GetOgUrl(doc);
                    page.OgImage = HtmlHelper.GetOgImage(doc);
                    page.Scanned = DateTime.Now;
                    if (scanlinks)
                    {
                        var links = HtmlHelper.GetLinks(doc);
                        page.Links.Clear();
                        foreach (var item in links)
                        {
                            if (Stop) return null;
                            Link link = new Link()
                            {
                                Href = item.Href,
                                Text = item.Text,
                                NoFollow = item.NoFollow
                            ,
                                PageId = page.Id,
                                Page = page,
                                SiteId = site.Id,
                                Site = site
                            };
                            if (site.Links.FirstOrDefault(x => x.Href == link.Href) != null) continue;
                            context.Add(link);
                            page.Links.Add(link);
                            site.Links.Add(link);
                            await context.SaveChangesAsync();
                            context.Update(page);
                            context.Update(site);
                            await context.SaveChangesAsync();
                            if (!link.NoFollow)
                            {
                                OnLog($"{DateTime.Now} Scan link {link.Href}");
                                await ScanPage(context, site, link.Href, rescan, scanlinks);
                            }
                            else { OnLog($"{DateTime.Now} NoFollow link {link.Href}"); }
                        }
                    }

                }
                else OnLog($"{DateTime.Now} Error code {response.StatusCode} read {path} ");
                context.Update(page);
                context.Update(site);
                await context.SaveChangesAsync();
                OnLog($"{DateTime.Now} Page scanned {page.PathAndQuary}");
                return page;
            }
            catch (Exception ex)
            {
                OnLog($"{DateTime.Now} Exception scanning Page {path}:\n{ex.Message}");
                return null;
            }
        }

        public void OnLog(string message) => LogDelegate?.Invoke(this, message);

    }

    public static class UriHelper
    {
        public static string GetSiteUri(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return string.Empty;
            url = url.ToLower();
            var uri = UriHelper.CreateUri(url);
            if (uri == null) return string.Empty;
            var Host = uri.Host;
            var Scheme = uri.Scheme;
            var Port = string.IsNullOrEmpty(uri.Port.ToString()) ? "" : $":{uri.Port}";
            var AbsoluteUri = @$"{Scheme}://{Host}{Port}";
            return AbsoluteUri;
        }
        public static Uri? CreateUri(string url)
        {
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri? uriResult))
                return uriResult;
            return null;
        }

        public static bool IsUrlBelongsToSite(string url, string siteDomain)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult))
                return uriResult.Host.EndsWith(siteDomain, StringComparison.OrdinalIgnoreCase);
            return false;
        }

        public static bool IsAbsolute(string url)
        {
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri uriResult))
                return uriResult.IsAbsoluteUri;
            return false;
        }

        public static Uri GetAbsoluteUri(string baseUri, string relativeUri)
        {
            Uri baseUriObj = new Uri(baseUri);
            Uri absoluteUri = new Uri(baseUriObj, relativeUri);
            return absoluteUri;
        }
    }
}

public static class HtmlHelper
{
    public static string GetTitle(HtmlDocument html)
    {
        var node = html.DocumentNode.SelectSingleNode("//title");
        return HtmlEntity.DeEntitize(node?.InnerText) ?? string.Empty;
    }
    public static string GetMetaDescription(HtmlDocument html)
    {
        var node = html.DocumentNode.SelectSingleNode("//meta[@name='description']");
        return node != null ? HtmlEntity.DeEntitize(node.GetAttributeValue("content", string.Empty)) : string.Empty;
    }
    public static string GetOgSiteName(HtmlDocument html)
    {
        var node = html.DocumentNode.SelectSingleNode("//meta[@property='og:site_name']");
        return node != null ? HtmlEntity.DeEntitize(node.GetAttributeValue("content", string.Empty)) : string.Empty;
    }
    public static string GetCanonicalLink(HtmlDocument html)
    {
        var node = html.DocumentNode.SelectSingleNode("//link[@rel='canonical']");
        return node != null ? HtmlEntity.DeEntitize(node.GetAttributeValue("href", string.Empty)) : string.Empty;
    }
    public static string GetOgUrl(HtmlDocument html)
    {
        var node = html.DocumentNode.SelectSingleNode("//meta[@property='og:url']");
        return node != null ? HtmlEntity.DeEntitize(node.GetAttributeValue("content", string.Empty)) : string.Empty;
    }
    public static string GetOgImage(HtmlDocument html)
    {
        var node = html.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
        return node != null ? HtmlEntity.DeEntitize(node.GetAttributeValue("content", string.Empty)) : string.Empty;
    }

    public static List<VMlink> GetLinks(HtmlDocument html)
    {
        var res = new List<VMlink>();
        var linkNodes = html.DocumentNode.SelectNodes("//a[@href]");
        if (linkNodes != null)
        {
            foreach (var linkNode in linkNodes)
            {
                var link = new VMlink();
                string rel = linkNode.GetAttributeValue("rel", string.Empty);
                link.NoFollow = rel.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                     .Contains("nofollow", StringComparer.OrdinalIgnoreCase);
                link.Href = linkNode.GetAttributeValue("href", string.Empty);
                link.Text = (HtmlEntity.DeEntitize(linkNode.InnerText)).Replace("\n", "").Replace("\t", "").Replace("\r", "");
                res.Add(link);
            }
        }
        return res;
    }
}

public static class PropertyHelper
{
    public static List<PropertyInfo> GetNonCollectionProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                   .Where(p => !typeof(System.Collections.IEnumerable).IsAssignableFrom(p.PropertyType)
                               || p.PropertyType == typeof(string))
                   .ToList();
    }
}


public class VMlink
{
    public string Href { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool NoFollow { get; set; } = false;
}

