using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using GifWin.Data;

namespace GifWin.Migrations
{
    [DbContext(typeof(GifWinContext))]
    [Migration("20151127030513_UsageDataStorage")]
    partial class UsageDataStorage
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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

            modelBuilder.Entity("GifWin.Data.GifUsage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("GifId");

                    b.Property<DateTimeOffset>("UsedAt");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "usages");
                });

            modelBuilder.Entity("GifWin.Data.GifTag", b =>
                {
                    b.HasOne("GifWin.Data.GifEntry")
                        .WithMany()
                        .HasForeignKey("GifId");
                });

            modelBuilder.Entity("GifWin.Data.GifUsage", b =>
                {
                    b.HasOne("GifWin.Data.GifEntry")
                        .WithMany()
                        .HasForeignKey("GifId");
                });
        }
    }
}
