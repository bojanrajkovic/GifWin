using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

namespace GifWin.Migrations
{
    public partial class AddIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql ("CREATE INDEX IX_GifTags_GifId ON tags(GifId)");
            migrationBuilder.Sql ("CREATE INDEX IX_GifUsages_GifId ON usages(GifId)");
            migrationBuilder.Sql ("CREATE INDEX IX_GifEntries_LastUsed ON gifs(LastUsed)");
            migrationBuilder.Sql ("CREATE INDEX IX_GifEntries_AddedAt ON gifs(AddedAt)");
            migrationBuilder.Sql ("CREATE INDEX IX_GifTags_Tag ON tags(Tag)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql ("DROP INDEX IX_GifTags_GifId");
            migrationBuilder.Sql ("DROP INDEX IX_GifUsages_GifId");
            migrationBuilder.Sql ("DROP INDEX IX_GifEntries_LastUsed");
            migrationBuilder.Sql ("DROP INDEX IX_GifEntries_AddedAt");
            migrationBuilder.Sql ("DROP INDEX IX_GifTags_Tag");
        }
    }
}
