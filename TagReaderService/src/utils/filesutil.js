const fs = require("fs");
const TagsUtil = require("./tagsutil");
const Artist = require("../models/artist");
const Album = require("../models/album");
const Genre = require("../models/genre");
const Tags = require("../models/tags");

const replaceWhitespaceWithUnderscore = value => value.replace(/\s/g, "_");
const replaceSlashWithDash = value => value.replace(/\//g, "-");

const getDirname = tags => {
  const genre = replaceSlashWithDash(tags.genre);
  const artist = replaceSlashWithDash(tags.artist);
  const album = replaceSlashWithDash(tags.album);
  return `${process.env.MUSIC_DIR}/${genre}/${artist}/${album}`;
};

const saveFile = file => {
  return new Promise(async resolve => {
    const jsonTags = await TagsUtil.getTags(file);
    const dirname = replaceWhitespaceWithUnderscore(getDirname(jsonTags));
    fs.mkdirSync(dirname, { recursive: true });

    const fileUri =
      dirname + "/" + replaceWhitespaceWithUnderscore(file.originalname);
    if (fs.existsSync(fileUri)) {
      return resolve();
    }
    fs.writeFileSync(fileUri, file.buffer);

    if (jsonTags.genre) {
      genre = await Genre.findOne({ name: jsonTags.genre });
      if (genre) {
        jsonTags.genre = genre;
      } else {
        jsonTags.genre = new Genre({ name: jsonTags.genre });
        await jsonTags.genre.save();
      }
    }

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

    if (jsonTags.album) {
      if (!jsonTags.artist) {
        return reject("Album must have an artist.");
      }

      const album = await Album.findOne({ name: jsonTags.album });
      if (album) {
        jsonTags.album = album;
      } else {
        jsonTags.album = new Album({ name: jsonTags.album });
      }

      if (jsonTags.genre) {
        jsonTags.album.genre = jsonTags.genre;
      }

      jsonTags.album.artist = jsonTags.artist;

      await jsonTags.album.save();
    }

    const tags = new Tags(jsonTags);
    tags.URI = fileUri;
    await tags.save();

    return resolve();
  });
};

const openFile = id => {
  return new Promise(async (resolve, reject) => {
    const tags = await Tags.findById(id);
    if (!tags) {
      return reject("Tags not found");
    }

    try {
      fs.openSync(tags.URI);
    } catch (error) {
      await Tags.deleteOne({ _id: id });
      return reject("File not found");
    }

    return resolve(tags.URI);
  });
};

module.exports = { saveFile, openFile };
