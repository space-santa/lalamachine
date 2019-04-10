﻿// <auto-generated />
using Lalamachine.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lalamachine.Migrations
{
    [DbContext(typeof(LalaContext))]
    partial class LalaContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.3-servicing-35854");

            modelBuilder.Entity("Lalamachine.Data.Album", b =>
                {
                    b.Property<int>("AlbumId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("AlbumId");

                    b.ToTable("Albums");
                });

            modelBuilder.Entity("Lalamachine.Data.Artist", b =>
                {
                    b.Property<int>("ArtistId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("ArtistId");

                    b.ToTable("Artists");
                });

            modelBuilder.Entity("Lalamachine.Data.ArtistTrack", b =>
                {
                    b.Property<int>("ArtistTrackId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ArtistId");

                    b.Property<int>("TrackId");

                    b.HasKey("ArtistTrackId");

                    b.HasIndex("ArtistId");

                    b.HasIndex("TrackId");

                    b.ToTable("ArtistTracks");
                });

            modelBuilder.Entity("Lalamachine.Data.Genre", b =>
                {
                    b.Property<int>("GenreId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("GenreId");

                    b.ToTable("Genres");
                });

            modelBuilder.Entity("Lalamachine.Data.GenreTrack", b =>
                {
                    b.Property<int>("GenreTrackId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("GenreId");

                    b.Property<int>("TrackId");

                    b.HasKey("GenreTrackId");

                    b.HasIndex("GenreId");

                    b.HasIndex("TrackId");

                    b.ToTable("GenreTracks");
                });

            modelBuilder.Entity("Lalamachine.Data.Track", b =>
                {
                    b.Property<int>("TrackId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AlbumId");

                    b.Property<string>("Comment");

                    b.Property<int>("DiscNumber");

                    b.Property<int>("Length");

                    b.Property<string>("Path");

                    b.Property<string>("Title");

                    b.Property<int>("TrackNumber");

                    b.Property<int>("Year");

                    b.HasKey("TrackId");

                    b.HasIndex("AlbumId");

                    b.ToTable("Tracks");
                });

            modelBuilder.Entity("Lalamachine.Data.ArtistTrack", b =>
                {
                    b.HasOne("Lalamachine.Data.Artist", "Artist")
                        .WithMany("ArtistTracks")
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Lalamachine.Data.Track", "Track")
                        .WithMany("ArtistTracks")
                        .HasForeignKey("TrackId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Lalamachine.Data.GenreTrack", b =>
                {
                    b.HasOne("Lalamachine.Data.Genre", "Genre")
                        .WithMany("GenreTracks")
                        .HasForeignKey("GenreId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Lalamachine.Data.Track", "Track")
                        .WithMany("GenreTracks")
                        .HasForeignKey("TrackId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Lalamachine.Data.Track", b =>
                {
                    b.HasOne("Lalamachine.Data.Album", "Album")
                        .WithMany("Tracks")
                        .HasForeignKey("AlbumId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
