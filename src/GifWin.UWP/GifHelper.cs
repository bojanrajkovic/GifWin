using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;

using GifWin.Core;
using GifWin.Core.Models;
using GifWin.Core.Data;
using GifWin.Core.Services;

namespace GifWin
{
    static class GifHelper
    {
        public static Task<string> GetOrMakeSavedAsync(GifEntry entry, byte[] frameData)
        {
            return Task.Run(async () => {
                try {
                    var expectedCacheName = $"{CacheHelper.ComputeFilesystemSafeHash(entry.Url)}.gif";
                    var cachedFileExists = CacheHelper.CacheContainsFile(expectedCacheName);

                    if (!cachedFileExists) {
                        using (var http = new System.Net.Http.HttpClient()) {
                            var download = await http.GetAsync(entry.Url);
                            if (download.IsSuccessStatusCode)
                                await CacheHelper.SaveFileToCacheAsync(
                                    await download.Content.ReadAsStreamAsync(),
                                    expectedCacheName,
                                    true
                                );
                        }
                    }

                    string fullCachePath = CacheHelper.GetFullPathToCachedFile(expectedCacheName);

                    if (entry.FirstFrame == null) {
#pragma warning disable CS4014
                        Task.Run(async () => {
                            var frame = await GetFrameDataAsync(fullCachePath, 1);
                            var db = ServiceContainer.Instance.GetRequiredService<GifWinDatabase>();
                            await db.UpdateFrameDataAsync(entry.Id, frame);
                        });
#pragma warning restore CS4014
                    }

                    return fullCachePath;
                } catch (Exception e) {
                    System.Diagnostics.Debug.WriteLine($"Exception while caching image {entry.Url}: {e.Message}");
                    return null;
                }
            });
        }

        public static async Task<FrameData> GetFrameDataAsync(string gifFile, uint frameNumber)
        {
            try {
                // Get the first frame.
                var storageFile = await StorageFile.GetFileFromPathAsync(gifFile);
                var decoder = await BitmapDecoder.CreateAsync(await storageFile.OpenReadAsync());

                if (frameNumber > decoder.FrameCount)
                    throw new ArgumentOutOfRangeException(
                        nameof(frameNumber),
                        $"Frame number {frameNumber} is greater than frame count {decoder.FrameCount}"
                    );

                var frame = await decoder.GetFrameAsync(frameNumber);

                // Now convert it to PNG.
                var ms = new MemoryStream();
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, ms.AsRandomAccessStream());
                var pixelData = await frame.GetPixelDataAsync();
                var pixelBytes = pixelData.DetachPixelData();

                encoder.SetPixelData(
                    frame.BitmapPixelFormat,
                    frame.BitmapAlphaMode,
                    frame.PixelWidth,
                    frame.PixelHeight,
                    frame.DpiX,
                    frame.DpiY,
                    pixelBytes
                );

                await encoder.FlushAsync();

                byte[] pngData = ms.ToArray();

                return new FrameData {
                    PngImage = pngData,
                    Width = (int)frame.PixelWidth,
                    Height = (int)frame.PixelHeight,
                };
            } catch {
                return null;
            }
        }
    }
}
