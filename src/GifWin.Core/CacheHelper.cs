using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace GifWin.Core
{
    public static class CacheHelper
    {
        static bool inited;
        static string cacheBasePath;

        public static void Init(string cacheBasePath)
        {
            CacheHelper.cacheBasePath = cacheBasePath;
            inited = true;
        }

        public static async Task SaveFileToCacheAsync(Stream stream, string cacheFileName, bool overwrite)
        {
            var destination = Path.Combine(cacheBasePath, cacheFileName);
            using (var file = File.Open(destination, overwrite ? FileMode.Create : FileMode.CreateNew)) {
                await stream.CopyToAsync(file);
            }
        }

        public static string ComputeHash(Stream stream)
        {
            using (var ha = SHA256.Create()) {
                var hash = ha.ComputeHash(stream);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
