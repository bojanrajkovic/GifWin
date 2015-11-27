using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GifWin.Data
{
    class GifWinDatabaseHelper : IDisposable
    {
        GifWinContext db;

        public GifWinDatabaseHelper()
        {
            db = new GifWinContext();
        }

        public async Task<int> ConvertGifWitLibraryAsync(GifWitLibrary librarySource)
        {
            foreach (var entry in librarySource) {
                var newGif = new GifEntry {
                    Url = entry.Url.ToString(),
                    AddedAt = DateTimeOffset.UtcNow,
                };
                
                foreach (var tag in entry.KeywordString.Split(' ')) {
                    newGif.Tags.Add(new GifTag { Tag = tag });
                }

                db.Gifs.Add(newGif);
            }

            return await db.SaveChangesAsync().ConfigureAwait(false);
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) {
                if (disposing) {
                    db.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
