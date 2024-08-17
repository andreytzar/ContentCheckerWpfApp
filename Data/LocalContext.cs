using ContentCheckerWpfApp.Models.DB;
using Microsoft.EntityFrameworkCore;

namespace ContentCheckerWpfApp.Data
{
    public class LocalContext : DbContext
    {
        public DbSet<Link> Links { get; set; }
        public DbSet<Site> Sites { get; set; }
        public DbSet<Page> Pages { get; set; }
        public LocalContext()
        {
           // Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ContentCheckerDBV1;Trusted_Connection=True;"));
        }

        public async Task<Site> GetSite(string siteUri)
        {
            try
            {
                var site = await Sites.FirstOrDefaultAsync(x => x.AbsoluteUri.ToLower() == siteUri.ToLower());

                if (site == null)
                {
                    site = new Site(siteUri);
                    Add(site);
                    await SaveChangesAsync();
                }
                await Entry(site).Collection(x => x.Links).LoadAsync();
                await Entry(site).Collection(x => x.Pages).LoadAsync();
                return site;
            }
            catch (Exception ex) { 
                var e = ex; return null; }
        }

        public async Task<Page?> AddPage(Site site, string path)
        {
            if (site == null) return null;
            if (!Entry(site).IsKeySet)
                Add(site);
            else
                Attach(site);

            var page = new Page() { PathAndQuary = path, SiteId = site.Id, Site = site };
            Add(page);
            site.Pages.Add(page);
            await SaveChangesAsync();
            return page;
        }
        //NotUse
        public async Task<Page?> GetPage(Site site, string path)
        {
            if (site == null) return null;
            if (!Entry(site).IsKeySet)
                Add(site);
            else
                Attach(site);
            var page = await Pages.Include(x => x.Site).Where(x => x.SiteId == site.Id).
                Where(x => string.Compare(x.PathAndQuary, path, StringComparison.Ordinal) == 0).FirstOrDefaultAsync();

            if (page == null)
            {
                page = new Page() { PathAndQuary = path, SiteId = site.Id, Site = site };
                Add(page);
                site.Pages.Add(page);
                await SaveChangesAsync();
            }
            return page;
        }
    }
}
