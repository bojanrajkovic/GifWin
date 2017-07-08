using System.Threading.Tasks;

using JetBrains.Annotations;

namespace GifWin.Core.Services
{
    [PublicAPI]
    public interface IPrompter
    {
        Task ShowMessageAsync(string title, string detail, string buttonText = "OK");
        Task<bool> PromptYesNoAsync(string title, string detail);
    }
}
