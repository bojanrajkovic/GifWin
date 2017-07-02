using System.Threading.Tasks;

using JetBrains.Annotations;

using GifWin.Core.Models;

namespace GifWin.Core.Services
{
    [PublicAPI]
    public interface IFrameDataService
    {
        Task<FrameData> GetFrameDataAsync(string gifFile, uint frameNumber);
    }
}
