
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContentCheckerWpfApp.Models.DB
{
    public class Link
    {
        [Key]
        public int Id { get; set; }
        public string Href { get; set; }=string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool NoFollow { get; set; } = false;
        public int PageId { get; set; }
        public DateTime DateTested { get; set; }
        public int LinkStatus { get; set; }
        [ForeignKey("PageLinkId")]
        public virtual Page? Page { get; set; }
        public int SiteId { get; set; }
        [ForeignKey("SiteLinkId")]
        public virtual Site? Site { get; set; }
        public string? MediaType { get; set; } = string.Empty;
    }
}
