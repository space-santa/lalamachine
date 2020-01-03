// This file exists because I couldn't make it work to replace functions in the module under test with mocks.
const fs = require("fs");

const Artist = require("../models/artist");
const Album = require("../models/album");
const Genre = require("../models/genre");
const Tags = require("../models/tags");

const replaceWhitespaceWithUnderscore = value => value.replace(/\s/g, "_");
const replaceNonWordWithDash = value => value.replace(/\W/g, "-");

const getDirname = tags => {
  const genre = tags.genre ? replaceNonWordWithDash(tags.genre) : "unknown";
  const artist = tags.artist ? replaceNonWordWithDash(tags.artist) : "unknown";
  const album = tags.album ? replaceNonWordWithDash(tags.album) : "unknown";
  const dirname = `${process.env.MUSIC_DIR}/${genre}/${artist}/${album}`;
  return dirname;
};

const getFilename = (dirname, originalname) => {
  return dirname + "/" + replaceWhitespaceWithUnderscore(originalname);
};

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

const writeFileToDisk = (file, dirname, fileUri) => {
  fs.mkdirSync(dirname, { recursive: true });
  if (fs.existsSync(fileUri)) {
    return;
  }
  fs.writeFileSync(fileUri, file.buffer);
};

module.exports = {
  writeFileToDisk,
  writeTagsToDatabase,
  getDirname,
  getFilename
};
