const Tags = require("./models/tags");
const Genre = require("./models/genre");
const Artist = require("./models/artist");
const Album = require("./models/album");

const saveGenre = async jsonTags => {
  if (jsonTags.genre) {
    genre = await Genre.findOne({ name: jsonTags.genre });
    if (genre) {
      jsonTags.genre = genre;
    } else {
      jsonTags.genre = new Genre({ name: jsonTags.genre });
      await jsonTags.genre.save();
    }
  }
};

const saveArtist = async jsonTags => {
  if (jsonTags.artist) {
    artist = await Artist.findOne({ name: jsonTags.artist });
    if (artist) {
      jsonTags.artist = artist;
    } else {
      jsonTags.artist = new Artist({ name: jsonTags.artist });
    }

    if (jsonTags.genre) {
      jsonTags.artist.genre = jsonTags.genre;
    }

    await jsonTags.artist.save();
  }
};

const saveAlbum = async jsonTags => {
  if (jsonTags.album) {
    if (!jsonTags.artist) {
      throw new Error("Album must have an artist.");
    }

    const album = await Album.findOne({
      name: jsonTags.album,
      artist: jsonTags.artist._id
    });
    if (album) {
      jsonTags.album = album;
    } else {
      jsonTags.album = new Album({ name: jsonTags.album });

      if (jsonTags.genre) {
        jsonTags.album.genre = jsonTags.genre;
      }

      jsonTags.album.artist = jsonTags.artist;

      await jsonTags.album.save();
    }
  }
};

const writeTagsToDatabase = async jsonTags => {
  await saveGenre(jsonTags);
  await saveArtist(jsonTags);
  await saveAlbum(jsonTags);
  const tags = new Tags(jsonTags);
  await tags.save();
};

const getTagsById = async id => {
  const tags = await Tags.findById(id);
  if (!tags) {
    throw new Error("Tags not found");
  }
  return tags;
};

const deleteTagsById = async id => {
  await Tags.deleteOne({ _id: id });
};

const getFilteredGenres = async (name = "") => {
  const filter = {};

  if (name) {
    filter.name = new RegExp(name);
  }

  const genres = await Genre.find(filter, "name");
  return genres;
};

const getFilteredArtists = async (name = "") => {
  const filter = {};

  if (name) {
    filter.name = new RegExp(name);
  }

  const artists = await Artist.find(filter, "name");
  return artists;
};

const getFilteredAlbums = async (name = "") => {
  const filter = {};

  if (name) {
    filter.name = new RegExp(name);
  }

  const albums = await Album.find(filter, "name artist").populate({
    path: "artist",
    select: "name"
  });
  return albums;
};

module.exports = {
  writeTagsToDatabase,
  getTagsById,
  deleteTagsById,
  getFilteredGenres,
  getFilteredArtists,
  getFilteredAlbums
};
