using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GifWin.Migrations
{
    public partial class AddWidthAndHeight : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "gifs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "gifs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_usages_GifId",
                table: "usages",
                column: "GifId");

            migrationBuilder.CreateIndex(
                name: "IX_tags_GifId",
                table: "tags",
                column: "GifId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_usages_GifId",
                table: "usages");

            migrationBuilder.DropIndex(
                name: "IX_tags_GifId",
                table: "tags");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "gifs");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "gifs");
        }
    }
}
