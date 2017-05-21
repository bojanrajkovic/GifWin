using System.Threading.Tasks;

using GifWin.Core.Models;

namespace GifWin.Core.Services
{
    public interface IFrameDataService
    {
        Task<FrameData> GetFrameDataAsync(string gifFile, uint frameNumber);
    }
}
