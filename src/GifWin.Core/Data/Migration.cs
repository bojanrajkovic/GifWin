using System.Data;
using System.Threading.Tasks;

namespace GifWin.Core.Data
{
    interface IMigration
    {
        Task ExecuteAsync(IDbConnection connection);
    }
}
