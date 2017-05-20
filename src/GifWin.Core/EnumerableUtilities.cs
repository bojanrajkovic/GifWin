using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GifWin.Core
{
    public static class EnumerableUtilities
    {
        public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (var t in self)
                action(t);
        }

        public static async Task ForEach<T>(this IEnumerable<T> self, Func<T, Task> action)
        {
            foreach (var t in self) {
                await action(t);
            }
        }
    }
}
