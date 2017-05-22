using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using GifWin.Core;
using GifWin.Core.Models;
using GifWin.Core.Data;
using GifWin.Core.Services;

namespace GifWin
{
    public static class GifHelper
    {
        static Dictionary<int, WeakReference<Task<string>>> cacheTaskCache =
            new Dictionary<int, WeakReference<Task<string>>>();

        public static bool TryGetCachedPathIfExists(GifEntry entry, out string cachedPath)
        {
            var expectedCacheName = $"{CacheHelper.ComputeFilesystemSafeHash(entry.Url)}.gif";
            cachedPath = CacheHelper.GetFullPathToCachedFile(expectedCacheName);
            return CacheHelper.CacheContainsFile(expectedCacheName);
        }

        public static Task<string> GetOrMakeSavedAsync(GifEntry entry, byte[] frameData)
        {
            var hasCachedTask = cacheTaskCache.TryGetValue(entry.Id, out var maybeCachedTask);
            if (hasCachedTask && maybeCachedTask != null && maybeCachedTask.TryGetTarget(out var cachedTask))
                return cachedTask;

            cachedTask = Task.Run(async () => {
                try {
                    var expectedCacheName = $"{CacheHelper.ComputeFilesystemSafeHash(entry.Url)}.gif";
                    var cachedFileExists = TryGetCachedPathIfExists(entry, out var fullCachePath);

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

                    if (entry.FirstFrame == null) {
#pragma warning disable CS4014
                        Task.Run(async () => {
                            var fds = ServiceContainer.Instance.GetRequiredService<IFrameDataService>();
                            var frame = await fds.GetFrameDataAsync(fullCachePath, 1);
                            var db = ServiceContainer.Instance.GetRequiredService<GifWinDatabase>();
                            await db.UpdateFrameDataAsync(entry.Id, frame);
                        });
#pragma warning restore CS4014
                    }

                    return fullCachePath;
                } catch (Exception e) {
                    ServiceContainer.Instance.GetLogger(nameof(GifHelper))
                                            ?.LogWarning(new EventId(), e, $"Exception while caching image {entry.Url}");
                    return null;
                }
            });

            cacheTaskCache[entry.Id] = new WeakReference<Task<string>>(cachedTask);
            return cachedTask;
        }
    }
}
