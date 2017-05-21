using System.Threading.Tasks;

namespace GifWin.Core.Services
{
    public interface IPrompter
    {
        Task<bool> PromptYesNoAsync(string title, string detail);
    }
}
