using System;
using Windows.Foundation.Metadata;

namespace GifWin.UWP
{
    static class ApiHelper
    {
        public static void RunIfPropertyIsPresent(string type, string property, Action action)
        {
            if (ApiInformation.IsPropertyPresent(type, property))
                action();
        }
    }
}
