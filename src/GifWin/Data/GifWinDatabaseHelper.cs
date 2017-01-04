using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        internal IQueryable<TResult> QueryGifs<TResult>(Expression<Func<GifEntry, bool>> filter,
                                                        Expression<Func<GifEntry, TResult>> map)
        {
            return db.Gifs.Where (filter).Select (map);
        }

        internal async Task<int> ConvertGifWitLibraryAsync (GifWitLibrary librarySource, IProgress<int> progress = null)
        {
            for (int i = 0; i < librarySource.Count; i++) {
                var entry = librarySource[i];

                if (db.Gifs.Any(ge => ge.Url.ToLower() == entry.Url.ToString().ToLower())) {
                    continue;
                }

                var newGif = new GifEntry {
                    Url = entry.Url.ToString (),
                    AddedAt = DateTimeOffset.UtcNow,
                };

                foreach (var tag in entry.KeywordString.Split (' ')) {
                    newGif.Tags.Add (new GifTag { Tag = tag });
                }

                db.Gifs.Add (newGif);

                progress?.Report(i+1);
            }

            progress?.Report(librarySource.Count);

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

        internal async Task RecordGifUsageAsync (int gifId, string searchTerm)
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

        internal async Task UpdateSavedFirstFrameDataAsync (int gifId, FrameData frameData)
        {
            var gif = await db.Gifs.SingleOrDefaultAsync (ge => ge.Id == gifId).ConfigureAwait (false);

            if (gif != null) {
                gif.FirstFrame = frameData.PngImage;
                gif.Width = frameData.Width;
                gif.Height = frameData.Height;


                await db.SaveChangesAsync ().ConfigureAwait (false);
            }
        }

        bool disposedValue = false;

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

        internal async Task DeleteGifAsync (int id)
        {
            var gif = await db.Gifs.Include (g => g.Tags).Include (g => g.Usages).SingleOrDefaultAsync (ge => ge.Id == id);

            if (gif != null) {
                db.Tags.RemoveRange (gif.Tags);
                db.Usages.RemoveRange (gif.Usages);
                db.Gifs.Remove (gif);

                await db.SaveChangesAsync ();
            }
        }
    }
}
