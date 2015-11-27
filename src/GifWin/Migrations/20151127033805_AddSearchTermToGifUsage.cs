using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

namespace GifWin.Migrations
{
    public partial class AddSearchTermToGifUsage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SearchTerm",
                table: "usages",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "SearchTerm", table: "usages");
        }
    }
}
