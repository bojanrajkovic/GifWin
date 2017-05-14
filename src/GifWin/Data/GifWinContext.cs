using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace GifWin.Data
{
    class GifWinContext : DbContext
    {
        public virtual DbSet<GifEntry> Gifs { get; set; }
        public virtual DbSet<GifTag> Tags { get; set; }
        public virtual DbSet<GifUsage> Usages { get; set; }

        static bool didAddLoggerFactory = false;
        static readonly object syncRoot = new object ();

        public GifWinContext ()
        {
            if (!didAddLoggerFactory) {
                lock (syncRoot) {
                    var lf = this.GetService<ILoggerFactory> ();
#if !CORE
                    lf.AddDebug ();
#endif
                    didAddLoggerFactory = true;
                }
            }
        }

        protected override void OnConfiguring (DbContextOptionsBuilder optionsBuilder)
        {
#if !CORE
            DirectoryInfo storage = new DirectoryInfo (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData), "GifWin", "Data"));
            storage.Create ();
            string source = Path.Combine (storage.ToString (), "gifwin.sqlite");
#else
            string source = "gifwin.sqlite";
#endif

			var csb = new SqliteConnectionStringBuilder {
                DataSource = source,
                Cache = SqliteCacheMode.Shared,
            };

            var connString = csb.ToString ();
            var conn = new SqliteConnection (connString);

            optionsBuilder.UseSqlite (conn, sqliteOptions => {
                sqliteOptions.UseRelationalNulls ();
            }).UseMemoryCache (null);
        }
    }
}
