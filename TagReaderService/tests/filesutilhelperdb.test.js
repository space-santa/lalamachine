const mockingoose = require("mockingoose").default;
const FilesUtilHelper = require("../src/utils/filesutilhelper");

const Genre = require("../src/models/genre");
const Artist = require("../src/models/artist");
const Album = require("../src/models/album");
const Tags = require("../src/models/tags");

const baseTags = {
  URI: "/bob/goes/to/town.mp3",
  duration: 234.56,
  title: "bob goes to town"
};

const getBaseTagsCopy = () => JSON.parse(JSON.stringify(baseTags));

beforeEach(() => {
  mockingoose.resetAll();
  mockingoose(Genre).toReturn({}, "save");
  mockingoose(Artist).toReturn({}, "save");
  mockingoose(Album).toReturn({}, "save");
  mockingoose(Tags).toReturn({}, "save");
});

test("Should create new Genre", async () => {
  const jsonTags = getBaseTagsCopy();
  jsonTags.genre = "bob album";

  await FilesUtilHelper.writeTagsToDatabase(jsonTags);

  expect(jsonTags.genre._id).not.toEqual(undefined);
});

test("Should use existing Genre if there is one", async () => {
  const jsonTags = getBaseTagsCopy();
  jsonTags.genre = "bob album";

  const _doc = {
    _id: "507f191e810c19729de860ea",
    name: "bob album"
  };
  mockingoose(Genre).toReturn(_doc, "findOne");

  await FilesUtilHelper.writeTagsToDatabase(jsonTags);

  expect(jsonTags.genre._id.toString()).toEqual(_doc._id);
});

test("Should create new Artist", async () => {
  const jsonTags = getBaseTagsCopy();
  jsonTags.artist = "bob";

  await FilesUtilHelper.writeTagsToDatabase(jsonTags);

  expect(jsonTags.artist._id).not.toEqual(undefined);
});

test("Should use existing Artist if there is one", async () => {
  const jsonTags = getBaseTagsCopy();
  jsonTags.artist = "bob";

  const _doc = {
    _id: "507f191e810c19729de860ea",
    name: "bob"
  };
  mockingoose(Artist).toReturn(_doc, "findOne");

  await FilesUtilHelper.writeTagsToDatabase(jsonTags);

  expect(jsonTags.artist._id.toString()).toEqual(_doc._id);
});

test("Should add Genre to Artist if there is one", async () => {
  const jsonTags = getBaseTagsCopy();
  jsonTags.artist = "bob";
  jsonTags.genre = "bob album";

  await FilesUtilHelper.writeTagsToDatabase(jsonTags);

  expect(jsonTags.artist.genre).not.toEqual(undefined);
});

test("Should create new Album if it has an artist", async () => {
  const jsonTags = getBaseTagsCopy();
  jsonTags.artist = "bob";
  jsonTags.album = "bob album";

  await FilesUtilHelper.writeTagsToDatabase(jsonTags);

  expect(jsonTags.album._id).not.toEqual(undefined);
});

test("Should use existing Album if there is one", async () => {
  const jsonTags = getBaseTagsCopy();
  jsonTags.artist = "bob";
  jsonTags.album = "bob album";

  const _doc = {
    _id: "507f191e810c19729de860ea",
    name: "bob album",
    artist: "507f191e810c19729de860ff"
  };
  mockingoose(Album).toReturn(_doc, "findOne");

  await FilesUtilHelper.writeTagsToDatabase(jsonTags);

  expect(jsonTags.album._id.toString()).toEqual(_doc._id);
});

test("Should add Genre to Album", async () => {
  const jsonTags = getBaseTagsCopy();
  jsonTags.artist = "bob";
  jsonTags.album = "bob album";
  jsonTags.genre = "bob genre";

  await FilesUtilHelper.writeTagsToDatabase(jsonTags);

  expect(jsonTags.album.genre.name).toEqual("bob genre");
});

test("Should throw error when album has no artist", async () => {
  try {
    await FilesUtilHelper.writeTagsToDatabase({ album: "bob album" });
  } catch (error) {
    expect(error.message).toEqual("Album must have an artist.");
  }
});

test("Should save Tags without error if the minimum required properties are provided", async () => {
  const jsonTags = getBaseTagsCopy();

  await FilesUtilHelper.writeTagsToDatabase(jsonTags);

  expect("got here because no errors").toEqual("got here because no errors");
});
