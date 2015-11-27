using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using GifWin.Data;

namespace GifWin.Migrations
{
    [DbContext(typeof(GifWinContext))]
    partial class GifWinContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348");

            modelBuilder.Entity("GifWin.Data.GifEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset?>("AddedAt");

                    b.Property<DateTimeOffset?>("LastUsed");

                    b.Property<string>("Url")
                        .IsRequired();

                    b.Property<int>("UsedCount");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "gifs");
                });

            modelBuilder.Entity("GifWin.Data.GifTag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("GifId");

                    b.Property<string>("Tag")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "tags");
                });

            modelBuilder.Entity("GifWin.Data.GifTag", b =>
                {
                    b.HasOne("GifWin.Data.GifEntry")
                        .WithMany()
                        .HasForeignKey("GifId");
                });
        }
    }
}
