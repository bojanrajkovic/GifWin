using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

namespace GifWin.Migrations
{
    public partial class AddFirstFrame : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "FirstFrame",
                table: "gifs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "FirstFrame", table: "gifs");
        }
    }
}
