using Microsoft.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GifWin.Data
{
    class GifWinDatabaseHelper : IDisposable
    {
        GifWinContext db;

        public GifWinDatabaseHelper ()
        {
            db = new GifWinContext ();
            db.Database.Migrate ();
        }

        public IQueryable<TResult> QueryGifs<TResult>(Expression<Func<GifEntry, bool>> filter,
                                                      Expression<Func<GifEntry, TResult>> map)
        {
            return db.Gifs.Where (filter).Select (map);
        }

        public async Task<int> ConvertGifWitLibraryAsync (GifWitLibrary librarySource)
        {
            foreach (var entry in librarySource) {
                var newGif = new GifEntry {
                    Url = entry.Url.ToString (),
                    AddedAt = DateTimeOffset.UtcNow,
                };

                foreach (var tag in entry.KeywordString.Split (' ')) {
                    newGif.Tags.Add (new GifTag { Tag = tag });
                }

                db.Gifs.Add (newGif);
            }

            return await db.SaveChangesAsync ().ConfigureAwait (false);
        }

        internal async Task<GifEntry> AddNewGifAsync (string gifUrl, string[] tags)
        {
            var existing = await db.Gifs.SingleOrDefaultAsync (ge => ge.Url.ToLower () == gifUrl.ToLower ());
            if (existing != null) {
                return existing;
            } else {
                var gifEntry = new GifEntry {
                    Url = gifUrl,
                    AddedAt = DateTimeOffset.UtcNow
                };

                foreach (var tag in tags) {
                    gifEntry.Tags.Add (new GifTag { Tag = tag });
                }

                var addedEntry = db.Gifs.Add (gifEntry);

                await db.SaveChangesAsync ();

                return addedEntry.Entity;
            }
        }

        public async Task RecordGifUsageAsync (int gifId, string searchTerm)
        {
            var gif = await db.Gifs.SingleOrDefaultAsync (ge => ge.Id == gifId).ConfigureAwait (false);

            if (gif != null) {
                var ts = DateTimeOffset.UtcNow;
                var usage = new GifUsage ();
                gif.LastUsed = usage.UsedAt = ts;
                gif.UsedCount++;
                usage.SearchTerm = searchTerm;
                gif.Usages.Add (usage);

                await db.SaveChangesAsync ().ConfigureAwait (false);
            }
        }

        internal async Task<IEnumerable<GifEntry>> GetGifsbyTagAsync (string[] tags)
        {
            return await db.Tags.Where (gt => tags.Contains (gt.Tag))
                           .Select (gt => gt.Gif)
                           .Distinct ()
                           .ToArrayAsync ()
                           .ConfigureAwait (false);
        }

        internal async Task UpdateSavedFirstFrameDataAsync (int gifId, byte[] frameData)
        {
            var gif = await db.Gifs.SingleOrDefaultAsync (ge => ge.Id == gifId).ConfigureAwait (false);

            if (gif != null) {
                gif.FirstFrame = frameData;

                await db.SaveChangesAsync ().ConfigureAwait (false);
            }
        }

        public async Task<IEnumerable<GifEntry>> LoadAllGifsAsync ()
        {
            var query = db.Gifs.Include (ge => ge.Tags);
            await query.LoadAsync ().ConfigureAwait (false);
            return await query.ToArrayAsync ();
        }

        private bool disposedValue = false;

        protected virtual void Dispose (bool disposing)
        {
            if (!disposedValue) {
                if (disposing) {
                    db.Dispose ();
                }

                disposedValue = true;
            }
        }

        public void Dispose ()
        {
            Dispose (true);
        }
    }
}
