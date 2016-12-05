using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using GifWin.Data;

namespace GifWin.Migrations
{
    [DbContext(typeof(GifWinContext))]
    partial class GifWinContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752");

            modelBuilder.Entity("GifWin.Data.GifEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset?>("AddedAt");

                    b.Property<byte[]>("FirstFrame");

                    b.Property<int>("Height");

                    b.Property<DateTimeOffset?>("LastUsed");

                    b.Property<string>("Url")
                        .IsRequired();

                    b.Property<int>("UsedCount");

                    b.Property<int>("Width");

                    b.HasKey("Id");

                    b.ToTable("gifs");
                });

            modelBuilder.Entity("GifWin.Data.GifTag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("GifId");

                    b.Property<string>("Tag")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("GifId");

                    b.ToTable("tags");
                });

            modelBuilder.Entity("GifWin.Data.GifUsage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("GifId");

                    b.Property<string>("SearchTerm");

                    b.Property<DateTimeOffset>("UsedAt");

                    b.HasKey("Id");

                    b.HasIndex("GifId");

                    b.ToTable("usages");
                });

            modelBuilder.Entity("GifWin.Data.GifTag", b =>
                {
                    b.HasOne("GifWin.Data.GifEntry", "Gif")
                        .WithMany("Tags")
                        .HasForeignKey("GifId");
                });

            modelBuilder.Entity("GifWin.Data.GifUsage", b =>
                {
                    b.HasOne("GifWin.Data.GifEntry", "Gif")
                        .WithMany("Usages")
                        .HasForeignKey("GifId");
                });
        }
    }
}
