using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Dapper;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

using GifWin.Core.Services;

namespace GifWin.Core.Data
{
    sealed class Migrator
    {
        [PublicAPI("Ostensibly not public API, but these are properties in the DB, just unused in code.")]
        class AppliedMigrationData
        {
            public int MigrationNumber { get; set; }
            public string MigrationName { get; set; }
            public string AppliedOn { get; set; }
        }

        readonly IDbConnection database;
        readonly Assembly migrationsAssembly;

        public Migrator(Assembly migrationsAssembly, IDbConnection database)
        {
            this.migrationsAssembly = migrationsAssembly;
            this.database = database;
        }

        public async Task<bool> MigrateAsync()
        {
            // Gather all the migrations.
            var migrations = migrationsAssembly.DefinedTypes.Where(
                ti => ti.GetCustomAttribute<MigrationAttribute>() != null &&
                      typeof(IMigration).GetTypeInfo().IsAssignableFrom(ti)
            ).Select(ti => (
                migrationInfo: ti.GetCustomAttribute<MigrationAttribute>(),
                migrationInstance: (IMigration)Activator.CreateInstance(ti.AsType())))
             .OrderBy(migData => migData.migrationInfo.MigrationNumber);

            await CreateMigrationsTableIfNeededAsync();

            var latestAppliedMigration = await GetLatestAppliedMigrationAsync();

            if (latestAppliedMigration == null)
                latestAppliedMigration = new AppliedMigrationData {
                    MigrationNumber = 0
                };

            var latestMigration = migrations.Last().migrationInfo.MigrationNumber;

            // If the latest migration is the latest applied migration, we're done.
            if (latestMigration == latestAppliedMigration.MigrationNumber)
                return true;

            // Find all the migrations since the latest applied.
            var migrationsSinceLastApplied = migrations.Where(
                mig => mig.migrationInfo.MigrationNumber > latestAppliedMigration.MigrationNumber
            ).ToList();

            var logger = ServiceContainer.Instance.GetLogger<Migrator>();

            logger.LogInformation($"Applying {migrationsSinceLastApplied.Count} migrations...");

            foreach (var migration in migrationsSinceLastApplied) {
                var applied = false;

                var transaction = database.BeginTransaction();
                var migrationInfo = migration.migrationInfo;
                logger.LogInformation($"Applying migration \"{migrationInfo.MigrationName}\" " +
                                      $"({migrationInfo.MigrationNumber})");
                var sw = Stopwatch.StartNew();
                try {
                    await migration.migrationInstance.ExecuteAsync(database);
                    transaction.Commit();
                    applied = true;

                    logger?.LogInformation($"Applied migration \"{migrationInfo.MigrationName}\" " +
                                           $"successfully in {sw.ElapsedMilliseconds}ms.");
                } catch (Exception e) {
                    transaction.Rollback();
                    logger?.LogWarning(new EventId(), e, $"Migration \"{migrationInfo.MigrationName}\" failed to apply.");
                } finally {
                    sw.Stop();
                }

                if (!applied)
                    return false;

                await InsertMigrationHistoryRecordAsync(migration.migrationInfo);
            }

            return true;
        }

        async Task InsertMigrationHistoryRecordAsync(MigrationAttribute migrationInfo)
        {
            await database.ExecuteAsync(@"
                INSERT INTO GifWinMigrationHistory(MigrationNumber, MigrationName, AppliedOn)
                VALUES (@MigrationNumber, @MigrationName, @appliedOn)
            ", new {
                migrationInfo.MigrationNumber,
                migrationInfo.MigrationName,
                appliedOn = DateTimeOffset.UtcNow.ToString("o")
            });
        }

        async Task<AppliedMigrationData> GetLatestAppliedMigrationAsync()
        {
            // Figure out what the latest one that was appplied is
            return await database.QuerySingleOrDefaultAsync<AppliedMigrationData>(@"
                SELECT * FROM GifWinMigrationHistory
                ORDER BY MigrationNumber DESC
                LIMIT 1
            ");
        }

        async Task CreateMigrationsTableIfNeededAsync()
        {
            // Create the migration history table if it doesn't exist.
            await database.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS ""GifWinMigrationHistory"" (
                    `MigrationNumber` INTEGER NOT NULL CONSTRAINT ""PK_MigrationHistory"" PRIMARY KEY,
                    `MigrationName`   TEXT NOT NULL,
                    `AppliedOn`       TEXT NOT NULL
                )
            ");
        }
    }
}
