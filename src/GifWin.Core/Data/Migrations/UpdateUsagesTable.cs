using System.Data;
using System.Threading.Tasks;

using Dapper;

namespace GifWin.Core.Data.Migrations
{
    [Migration(2, MigrationName = "Add search provider to usage table")]
    sealed class AddSearchProvider : IMigration
    {
        const string AddSearchProviderSql = "ALTER TABLE usages ADD COLUMN SearchProvider TEXT";

        public async Task ExecuteAsync(IDbConnection connection) =>
            await connection.ExecuteAsync(AddSearchProviderSql);
    }
}
