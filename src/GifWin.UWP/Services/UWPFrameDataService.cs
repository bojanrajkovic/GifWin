using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;

using GifWin.Core.Models;
using GifWin.Core.Services;

namespace GifWin.UWP.Services
{
    class UWPFrameDataService : IFrameDataService
    {
        public async Task<FrameData> GetFrameDataAsync(string gifFile, uint frameNumber)
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
