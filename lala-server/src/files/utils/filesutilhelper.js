// This file exists because I couldn't make it work to replace functions in the module under test with mocks.
import { mkdirSync, existsSync, writeFileSync } from "fs";

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

const writeFileToDisk = (file, dirname, fileUri) => {
  mkdirSync(dirname, { recursive: true });
  if (existsSync(fileUri)) {
    return;
  }
  writeFileSync(fileUri, file.buffer);
};

export default {
  writeFileToDisk,
  getDirname,
  getFilename
};
