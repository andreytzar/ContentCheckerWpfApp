﻿// <auto-generated />
using System;
using ContentCheckerWpfApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ContentCheckerWpfApp.Migrations
{
    [DbContext(typeof(LocalContext))]
    [Migration("20240818105333_linkMT")]
    partial class linkMT
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("ContentCheckerWpfApp.Models.DB.Link", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("DateTested")
                        .HasColumnType("datetime2");

                    b.Property<string>("Href")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("LinkStatus")
                        .HasColumnType("int");

                    b.Property<string>("MediaType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("NoFollow")
                        .HasColumnType("bit");

                    b.Property<int>("PageId")
                        .HasColumnType("int");

                    b.Property<int?>("PageLinkId")
                        .HasColumnType("int");

                    b.Property<int>("SiteId")
                        .HasColumnType("int");

                    b.Property<int?>("SiteLinkId")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("PageLinkId");

                    b.HasIndex("SiteLinkId");

                    b.ToTable("Links");
                });

            modelBuilder.Entity("ContentCheckerWpfApp.Models.DB.Page", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AbsoluteUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CanonicalLink")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MediaType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OgImage")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OgSiteName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OgUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PathAndQuary")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("Scanned")
                        .HasColumnType("datetime2");

                    b.Property<int>("SiteId")
                        .HasColumnType("int");

                    b.Property<int>("StatusCode")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("SiteId");

                    b.ToTable("Pages");
                });

            modelBuilder.Entity("ContentCheckerWpfApp.Models.DB.Site", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AbsoluteUri")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CurrentPage")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Host")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Port")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Scheme")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Sites");
                });

            modelBuilder.Entity("ContentCheckerWpfApp.Models.DB.Link", b =>
                {
                    b.HasOne("ContentCheckerWpfApp.Models.DB.Page", "Page")
                        .WithMany("Links")
                        .HasForeignKey("PageLinkId");

                    b.HasOne("ContentCheckerWpfApp.Models.DB.Site", "Site")
                        .WithMany("Links")
                        .HasForeignKey("SiteLinkId");

                    b.Navigation("Page");

                    b.Navigation("Site");
                });

            modelBuilder.Entity("ContentCheckerWpfApp.Models.DB.Page", b =>
                {
                    b.HasOne("ContentCheckerWpfApp.Models.DB.Site", "Site")
                        .WithMany("Pages")
                        .HasForeignKey("SiteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Site");
                });

            modelBuilder.Entity("ContentCheckerWpfApp.Models.DB.Page", b =>
                {
                    b.Navigation("Links");
                });

            modelBuilder.Entity("ContentCheckerWpfApp.Models.DB.Site", b =>
                {
                    b.Navigation("Links");

                    b.Navigation("Pages");
                });
#pragma warning restore 612, 618
        }
    }
}
