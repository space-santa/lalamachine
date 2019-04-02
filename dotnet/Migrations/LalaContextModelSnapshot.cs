﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using dotnet.Data;

namespace dotnet.Migrations
{
    [DbContext(typeof(LalaContext))]
    partial class LalaContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.3-servicing-35854");

            modelBuilder.Entity("dotnet.Data.Album", b =>
                {
                    b.Property<string>("AlbumId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("AlbumId");

                    b.ToTable("Albums");
                });

            modelBuilder.Entity("dotnet.Data.Artist", b =>
                {
                    b.Property<string>("ArtistId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("ArtistId");

                    b.ToTable("Artists");
                });

            modelBuilder.Entity("dotnet.Data.ArtistsOfTrack", b =>
                {
                    b.Property<string>("ArtistsOfTrackId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ArtistId");

                    b.Property<string>("TrackId");

                    b.HasKey("ArtistsOfTrackId");

                    b.ToTable("ArtistsOfTracks");
                });

            modelBuilder.Entity("dotnet.Data.Genre", b =>
                {
                    b.Property<string>("GenreId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("GenreId");

                    b.ToTable("Genres");
                });

            modelBuilder.Entity("dotnet.Data.GenresOfTrack", b =>
                {
                    b.Property<string>("GenresOfTrackId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("GenreId");

                    b.Property<string>("TrackId");

                    b.HasKey("GenresOfTrackId");

                    b.ToTable("GenresOfTracks");
                });

            modelBuilder.Entity("dotnet.Data.Track", b =>
                {
                    b.Property<string>("TrackId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AlbumId");

                    b.Property<string>("Comment");

                    b.Property<int>("DiscNumber");

                    b.Property<int>("Length");

                    b.Property<string>("Path");

                    b.Property<string>("Title");

                    b.Property<int>("TrackNumber");

                    b.Property<int>("Year");

                    b.HasKey("TrackId");

                    b.ToTable("Tracks");
                });
#pragma warning restore 612, 618
        }
    }
}
