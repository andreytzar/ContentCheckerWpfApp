using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;


namespace ContentCheckerWpfApp.Models.DB
{
    public class Page:INotifyPropertyChanged
    {
        [Key]
        public int Id { get; set; }
        public string PathAndQuary { get; set; } = string.Empty;
        public string AbsoluteUrl { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CanonicalLink { get; set; } = string.Empty;
        public string OgSiteName { get; set; } = string.Empty;
        public string OgUrl { get; set; } = string.Empty;
        public string OgImage { get; set; } = string.Empty;
        public int StatusCode { get; set; } =-1;
        public DateTime? Scanned { get; set; }
        public string? MediaType { get; set; }=string.Empty;
        public int SiteId { get; set; }
        [ForeignKey("SiteId")]
        public virtual Site? Site { get; set; }

        public override string ToString() => PathAndQuary;

        public virtual ObservableCollection<Link> Links { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
