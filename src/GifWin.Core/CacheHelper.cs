using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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

        private static void CheckInited()
        {
            if (!inited)
                throw new InvalidOperationException("Cache helper has not been inited.");
        }

        public static string GetFullPathToCachedFile(string cacheFileName)
        {
            CheckInited();
            return Path.Combine(cacheBasePath, cacheFileName);
        }

        public static bool CacheContainsFile(string cacheFileName)
        {
            CheckInited();
            return File.Exists(GetFullPathToCachedFile(cacheFileName));
        }

        public static async Task SaveFileToCacheAsync(Stream stream, string cacheFileName, bool overwrite)
        {
            CheckInited();
            var destination = Path.Combine(cacheBasePath, cacheFileName);
            using (var file = File.Open(destination, overwrite ? FileMode.Create : FileMode.CreateNew)) {
                await stream.CopyToAsync(file);
            }
        }

        public static string ComputeFilesystemSafeHash(string str)
        {
            using (var ha = SHA256.Create()) {
                var hash = ha.ComputeHash(Encoding.UTF8.GetBytes(str));
                var base64 = Convert.ToBase64String(hash);
                Path.GetInvalidFileNameChars().ForEach(c => {
                    base64 = base64.Replace(c, '-');
                });
                return base64;
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
