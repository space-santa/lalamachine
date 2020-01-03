const fs = require("fs");
const FilesUtilHelper = require("./filesutilhelper");
const TagsUtil = require("./tagsutil");
const Tags = require("../models/tags");

const saveFile = async file => {
  const jsonTags = await TagsUtil.getTags(file);
  const dirname = FilesUtilHelper.getDirname(jsonTags);
  jsonTags.URI = FilesUtilHelper.getFilename(dirname, file.originalname);

  FilesUtilHelper.writeFileToDisk(file, dirname, jsonTags.URI);

  await FilesUtilHelper.writeTagsToDatabase(jsonTags);
};

const openFile = async id => {
  const tags = await Tags.findById(id);
  if (!tags) {
    throw new Error("Tags not found");
  }

  try {
    fs.openSync(tags.URI);
  } catch (error) {
    await Tags.deleteOne({ _id: id });
    throw new Error("File not found");
  }

  return tags.URI;
};

module.exports = {
  saveFile,
  openFile
};
