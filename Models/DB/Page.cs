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
        public string PathAndQuary { get => _PathAndQuary; set { _PathAndQuary = value; OnPropertyChanged(); } } 
        public string AbsoluteUrl { get => _AbsoluteUrl; set { _AbsoluteUrl = value; OnPropertyChanged(); } } 
        public string Title { get => _Title; set { _Title = value; OnPropertyChanged(); } } 
        public string Description { get => _Description; set { _Description = value; OnPropertyChanged(); } } 
        public string CanonicalLink { get => _CanonicalLink; set { _CanonicalLink = value; OnPropertyChanged(); } } 
        public string OgSiteName { get => _OgSiteName; set { _OgSiteName = value; OnPropertyChanged(); } } 
        public string OgUrl { get => _OgUrl; set { _OgUrl = value; OnPropertyChanged(); } } 
        public string OgImage { get => _OgImage; set { _OgImage = value; OnPropertyChanged(); } } 
        public int StatusCode { get => _StatusCode; set { _StatusCode = value; OnPropertyChanged(); } }
        public DateTime? Scanned { get => _Scanned; set { _Scanned = value; OnPropertyChanged(); } }
        public string? MediaType { get; set; }=string.Empty;
        public int SiteId { get; set; }
        [ForeignKey("SiteId")]
        public virtual Site? Site { get; set; }
        
        private int _Id { get; set; }
        private string _PathAndQuary { get; set; } = string.Empty;
        private string _AbsoluteUrl { get; set; } = string.Empty;
        private string _Title { get; set; } = string.Empty;
        private string _Description { get; set; } = string.Empty;
        private string _CanonicalLink { get; set; } = string.Empty;
        private string _OgSiteName { get; set; } = string.Empty;
        private string _OgUrl { get; set; } = string.Empty;
        private string _OgImage { get; set; } = string.Empty;
        private int _StatusCode { get; set; }
        private DateTime? _Scanned { get; set; } = null;

        public override string ToString() => PathAndQuary;

        public virtual ObservableCollection<Link> Links { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
