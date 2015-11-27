using Microsoft.Data.Entity;
using Microsoft.Data.Sqlite;

namespace GifWin.Data
{
    class GifWinContext : DbContext
    {
        public virtual DbSet<GifEntry> Gifs { get; set; }
        public virtual DbSet<GifTag> Tags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var csb = new SqliteConnectionStringBuilder {
                DataSource = "gifwin.sqlite",
                Cache = SqliteConnectionCacheMode.Shared,
            };
            var connString = csb.ToString();
            var conn = new SqliteConnection(connString);

            optionsBuilder.UseSqlite(conn);
        }
    }
}
