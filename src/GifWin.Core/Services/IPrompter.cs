using System.Threading.Tasks;

namespace GifWin.Core.Services
{
    public interface IPrompter
    {
        Task ShowMessageAsync(string title, string detail, string buttonText = "OK");
        Task<bool> PromptYesNoAsync(string title, string detail);
    }
}
