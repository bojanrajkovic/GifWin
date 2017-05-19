using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace GifWin.Core.Data.Migrations
{
    [Migration(1, MigrationName = "Create initial database structure")]
    class InitialDatabaseCreation : IMigration
    {
        const string CreateGifsTable = @"
            CREATE TABLE IF NOT EXISTS ""gifs"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_GifEntry"" PRIMARY KEY AUTOINCREMENT, 
                ""AddedAt"" TEXT, 
                ""LastUsed"" TEXT, 
                ""Url"" TEXT NOT NULL, 
                ""UsedCount"" INTEGER NOT NULL , 
                ""FirstFrame"" BLOB, 
                ""Height"" INTEGER NOT NULL DEFAULT 0, 
                ""Width"" INTEGER NOT NULL DEFAULT 0
            )
        ";

        const string CreateTagsTable = @"
            CREATE TABLE IF NOT EXISTS ""tags"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_GifTag"" PRIMARY KEY AUTOINCREMENT,
                ""GifId"" INTEGER,
                ""Tag"" TEXT NOT NULL,
                CONSTRAINT ""FK_GifTag_GifEntry_GifId"" FOREIGN KEY (""GifId"") REFERENCES ""gifs"" (""Id"") ON DELETE CASCADE
            )
        ";

        const string CreateUsagesTable = @"
            CREATE TABLE IF NOT EXISTS ""usages"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_GifUsage"" PRIMARY KEY AUTOINCREMENT,
                ""GifId"" INTEGER,
                ""UsedAt"" TEXT NOT NULL,
                ""SearchTerm"" TEXT,
                CONSTRAINT ""FK_GifUsage_GifEntry_GifId"" FOREIGN KEY (""GifId"") REFERENCES ""gifs"" (""Id"") ON DELETE CASCADE
            )
        ";

        static readonly string[] CreateIndexes = new[] {
            "CREATE INDEX IF NOT EXISTS IX_GifEntries_AddedAt ON gifs(AddedAt)",
            "CREATE INDEX IF NOT EXISTS IX_GifEntries_LastUsed ON gifs(LastUsed)",
            "CREATE INDEX IF NOT EXISTS IX_GifTags_GifId ON tags(GifId)",
            "CREATE INDEX IF NOT EXISTS IX_GifTags_Tag ON tags(Tag)",
            "CREATE INDEX IF NOT EXISTS IX_GifUsages_GifId ON usages(GifId)"
        };

        public async Task ExecuteAsync(IDbConnection connection)
        {
            await connection.ExecuteAsync(CreateGifsTable);
            await connection.ExecuteAsync(CreateTagsTable);
            await connection.ExecuteAsync(CreateUsagesTable);
            await CreateIndexes.ForEach(async statement => await connection.ExecuteAsync(statement));
        }
    }
}
