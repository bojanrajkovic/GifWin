using System;

namespace GifWin.Core.Data
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class MigrationAttribute : Attribute
    {
        public MigrationAttribute(int migrationNumber) =>
            MigrationNumber = migrationNumber;

        public int MigrationNumber { get; }

        public string MigrationName { get; set; }
    }
}
