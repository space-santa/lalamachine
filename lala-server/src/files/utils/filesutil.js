import { openSync } from "fs";
import {
  getDirname,
  getFilename,
  writeFileToDisk,
  writeTagsToDatabase
} from "./filesutilhelper";
import { getTags } from "./tagsutil";
import { getTagsById, deleteTagsById } from "../db/interface";

const saveFile = async file => {
  const jsonTags = await getTags(file);
  const dirname = getDirname(jsonTags);
  jsonTags.URI = getFilename(dirname, file.originalname);

  writeFileToDisk(file, dirname, jsonTags.URI);

  await writeTagsToDatabase(jsonTags);
};

const openFile = async id => {
  const tags = await getTagsById(id);

  try {
    openSync(tags.URI);
  } catch (error) {
    await deleteTagsById(id);
    throw new Error("File not found");
  }

  return tags.URI;
};

export default {
  saveFile,
  openFile
};
