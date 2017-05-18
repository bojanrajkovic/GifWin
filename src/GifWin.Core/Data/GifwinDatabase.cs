using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Dapper;
using Microsoft.Data.Sqlite;

using GifWin.Core.Models;
using System.Linq;

namespace GifWin.Core.Data
{
    public class GifwinDatabase : IDisposable
    {
        const string dateTimeOffsetFormat = "yyyy-MM-dd hh:mm:ss.fffffffzzzz";

        IDbConnection connection;

        public GifwinDatabase(string databaseFile)
        {
            connection = new SqliteConnection($"Filename={databaseFile}");
            connection.Open();
        }

        public async Task<IEnumerable<GifEntry>> GetAllGifsAsync()
        {
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

        public async Task<IEnumerable<GifTag>> GetAllTagsAsync() =>
            await connection.QueryAsync<GifTag>("SELECT * FROM Tags");

        public async Task<IEnumerable<GifEntry>> GetGifsByTagAsync(string tag) =>
            await connection.QueryAsync<GifTag, GifEntry, GifEntry>(
                "SELECT g.* FROM Tags t JOIN Gifs g ON t.GifId = g.Id WHERE t.Tag = @tag",
                (dbTag, entry) => entry,
                new { tag }
            );

        public async Task<GifEntry> GetGifByIdAsync(int id)
        {
            GifEntry realEntry = null;
            await connection.QueryAsync<GifTag, GifEntry, GifEntry>(
                "SELECT g.* FROM Tags t JOIN Gifs g ON t.GifId = g.Id WHERE g.Id = @id",
                (tag, entry) => {
                    realEntry = realEntry ?? entry;
                    entry.Tags = (entry.Tags ?? Array.Empty<GifTag>()).Concat(new[] { tag });
                    return entry;
                },
                new { id }
            );
            return realEntry;
        }

        public async Task<GifEntry> UpdateFrameDataAsync(int gifId, FrameData data)
        {
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

        #region IDisposable Support
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed) {
                if (disposing) {
                    connection.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}
