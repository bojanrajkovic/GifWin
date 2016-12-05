using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GifWin.Migrations
{
    public partial class BaseDatabaseStructure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gifs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AddedAt = table.Column<DateTimeOffset>(nullable: true),
                    LastUsed = table.Column<DateTimeOffset>(nullable: true),
                    Url = table.Column<string>(nullable: false),
                    UsedCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GifEntry", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GifId = table.Column<int>(nullable: true),
                    Tag = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GifTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GifTag_GifEntry_GifId",
                        column: x => x.GifId,
                        principalTable: "gifs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("tags");
            migrationBuilder.DropTable("gifs");
        }
    }
}
