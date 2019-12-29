const fs = require("fs");
const TagsUtil = require("./tagsutil");
const Tags = require("../models/tags");

const replaceWhitespaceWithUnderscore = value => value.replace(/\s/g, "_");

const getDirname = tags =>
  `${process.env.MUSIC_DIR}/${tags.genre}/${tags.artist}/${tags.album}`;

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
