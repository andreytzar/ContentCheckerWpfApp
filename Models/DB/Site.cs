using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace ContentCheckerWpfApp.Models.DB
{
    public class Site
    {
        [Key]
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Host { get; set; }=string.Empty;
        public string AbsoluteUri {  get; set; } = string.Empty;
        public string Scheme { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;
        public string CurrentPage { get; set; } = string.Empty;
        public virtual ObservableCollection<Page> Pages { get; set; } = new();
        public virtual ObservableCollection<Link> Links { get; set; } = new();
        public override string ToString() => AbsoluteUri;
        public Site(string url)
        {
            Url= url.ToLower();
            var uri=UriHelper.CreateUri(url);
            if (uri == null) return;
            Host = uri.Host;
            Scheme = uri.Scheme;
            Port = string.IsNullOrEmpty(uri.Port.ToString())?"":$":{uri.Port}";
            AbsoluteUri=UriHelper.GetSiteUri(url);
        }
    }
}
