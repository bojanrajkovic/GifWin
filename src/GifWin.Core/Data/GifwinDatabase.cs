using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

using GifWin.Core.Models;
using GifWin.Core.Services;

namespace GifWin.Core.Data
{
    public sealed class GifWinDatabase : IDisposable
    {
        const string dateTimeOffsetFormat = "yyyy-MM-dd hh:mm:ss.fffffffzzzz";

        static readonly string[] Pragmas = new[] {
            // Automatic indexing is good.
            "PRAGMA automatic_index = true",
            // We definitely want foreign-keys.
            "PRAGMA foreign_keys = true",
            // WAL journaling makes the DB more resilient to crashes/etc.
            "PRAGMA journal_mode = wal",
            // The "NORMAL" synchronous mode is safe in WAL mode
            "PRAGMA synchronous = NORMAL",
            // Allow using a few threads.
            $"PRAGMA threads = {Environment.ProcessorCount}"
        };

        IDbConnection connection;

        public GifWinDatabase(string databaseFile)
        {
            // Open the connection.
            connection = new SqliteConnection($"Filename={databaseFile}");
            connection.Open();

            // Set up pragmas.
            Pragmas.ForEach(pragma => connection.Execute(pragma));
        }

        public async Task<bool> ExecuteMigrationsAsync()
        {
            CheckDisposed();

            var migrator = new Migrator(typeof(GifWinDatabase).GetTypeInfo().Assembly, connection);
            return await migrator.MigrateAsync();
        }

        public async Task<IEnumerable<GifEntry>> GetAllGifsAsync()
        {
            CheckDisposed();

            var gifs = (await connection.QueryAsync<GifEntry>("SELECT * FROM Gifs")).ToList();
            var tags = (await connection.QueryAsync<GifTag>("SELECT * FROM Tags"))
                                        .ToLookup(gt => gt.GifId);
            var usages = (await connection.QueryAsync<GifUsage>("SELECT * FROM Usages"))
                                          .ToLookup(gu => gu.GifId);

            foreach (var gif in gifs) {
                gif.Tags = tags.Contains(gif.Id) ? tags[gif.Id] : Array.Empty<GifTag>();
                gif.Usages = usages.Contains(gif.Id) ? usages[gif.Id] : Array.Empty<GifUsage>();
            }

            return gifs;
        }

        public async Task<IEnumerable<GifTag>> GetAllTagsAsync()
        {
            CheckDisposed();

            return await connection.QueryAsync<GifTag>("SELECT * FROM Tags");
        }

        public async Task<IEnumerable<GifEntry>> GetGifsByTagAsync(string tag)
        {
            CheckDisposed();

            return await connection.QueryAsync<GifEntry>(
                "SELECT g.* FROM Tags t JOIN Gifs g ON t.GifId = g.Id WHERE t.Tag = @tag",
                new { tag }
            );
        }

        public async Task<IEnumerable<GifEntry>> GetGifsbyTagAsync(string[] filterArray)
        {
            CheckDisposed();

            return await connection.QueryAsync<GifEntry>(
                "SELECT g.* FROM Tags t JOIN Gifs g ON t.GifId = g.Id WHERE t.Tag IN @filterArray",
                new { filterArray }
            );
        }

        public async Task<GifEntry> GetGifByIdAsync(int id)
        {
            CheckDisposed();

            GifEntry realEntry = null;
            await connection.QueryAsync<GifTag, GifEntry, GifEntry>(
                "SELECT t.*, g.* FROM Tags t JOIN Gifs g ON t.GifId = g.Id WHERE g.Id = @id",
                (tag, entry) => {
                    realEntry = realEntry ?? entry;
                    entry.Tags = (realEntry.Tags ?? Array.Empty<GifTag>()).Concat(new[] { tag });
                    realEntry = entry;
                    return entry;
                },
                new { id }
                //splitOn: "g.Id"
            );
            return realEntry;
        }

        public async Task RecordGifUsageAsync(int gifId, string searchTerm)
        {
            CheckDisposed();

            var lastUsed = DateTimeOffset.UtcNow.ToString(dateTimeOffsetFormat);
            await connection.ExecuteAsync(@"
                UPDATE Gifs
                SET LastUsed = @lastUsed, UsedCount = UsedCount + 1
                WHERE Id = @gifId
            ", new {
                gifId,
                lastUsed,
            });
            await connection.ExecuteAsync(@"
                INSERT INTO Usages(GifId, UsedAt, SearchTerm)
                VALUES(@gifId, @lastUsed, @searchTerm)
            ", new {
                gifId,
                lastUsed,
                searchTerm
            });
        }

        public async Task DeleteGifAsync(int gifId)
        {
            CheckDisposed();

            await connection.ExecuteAsync(@"
                DELETE FROM Gifs WHERE Id = @gifId
            ", new {
                gifId
            });
        }

        public async Task<GifEntry> UpdateFrameDataAsync(int gifId, FrameData data)
        {
            CheckDisposed();

            await connection.ExecuteAsync(@"
                UPDATE Gifs
                SET FirstFrame = @frame, Width = @width, Height = @height
                WHERE Id = @gifId
            ", new {
                gifId,
                frame = data.PngImage,
                width = data.Width,
                height = data.Height
            });
            return await GetGifByIdAsync(gifId);
        }

        public async Task<GifEntry> AddGifEntryAsync(string url, string[] tags)
        {
            CheckDisposed();

            var newGifId = await connection.QuerySingleAsync<int>(@"
                INSERT INTO Gifs(AddedAt, Url, UsedCount) VALUES(@addedAt, @url, @usedCount);
                SELECT last_insert_rowid();
            ", new {
                addedAt = DateTimeOffset.UtcNow.ToString(dateTimeOffsetFormat),
                url,
                usedCount = 0
            });
            foreach (var tag in tags) {
                await connection.ExecuteAsync(
                    "INSERT INTO Tags(GifId, Tag) VALUES (@newGifId, @tag);",
                    new { newGifId, tag }
                );
            }
            return await GetGifByIdAsync(newGifId);
        }

        public void ForceFlush()
        {
            CheckDisposed();

            try {
                connection.Execute("PRAGMA wal_checkpoint(FULL);");
            } catch (Exception e) {
                ServiceContainer.Instance.GetLogger<GifWinDatabase>()
                               ?.LogWarning(null, e, "Could not execute wal_checkpoint.");
            }
        }

        public void Optimize()
        {
            CheckDisposed();

            try {
                connection.Execute("PRAGMA optimize;");
            } catch (Exception e) {
                ServiceContainer.Instance.GetLogger<GifWinDatabase>()
                               ?.LogWarning(null, e, "Could not execute optimized.");
            }
        }

        #region IDisposable Support
        private bool disposed = false;

        void CheckDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(GifWinDatabase));
        }

        void Dispose(bool disposing)
        {
            if (!disposed) {
                if (disposing) {
                    // It's recommended to do this before closing the DB connection,
                    // in the SQLite documentation: http://www.sqlite.org/pragma.html#pragma_optimize
                    Optimize();
                    connection.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose() => Dispose(true);
        #endregion

    }
}
