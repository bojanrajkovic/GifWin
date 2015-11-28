using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;

namespace GifWin.Data
{
    class GifWinContext : DbContext
    {
        public virtual DbSet<GifEntry> Gifs { get; set; }
        public virtual DbSet<GifTag> Tags { get; set; }

        public GifWinContext()
        {
            var lf = this.GetService<ILoggerFactory> ();
            lf.AddDebug (LogLevel.Debug);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var csb = new SqliteConnectionStringBuilder {
                DataSource = "gifwin.sqlite",
                Cache = SqliteConnectionCacheMode.Shared,
            };
            var connString = csb.ToString();
            var conn = new SqliteConnection(connString);

            optionsBuilder.UseSqlite(conn).UseRelationalNulls();
        }
    }
}
