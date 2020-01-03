const mockfs = require("mock-fs");
const TagsUtil = require("../src/utils/tagsutil");
const FilesUtilHelper = require("../src/utils/filesutilhelper");

const mockingoose = require("mockingoose").default;
const Tags = require("../src/models/tags");
const FilesUtil = require("../src/utils/filesutil");
const getTestTags = require("./fixtures/tagsfactory");

test("Should not find tags if there are no tags", async () => {
  mockingoose(Tags).toReturn(undefined, "findOne");
  try {
    await FilesUtil.openFile("507f191e810c19729de860ea");
  } catch (error) {
    expect(error.message).toEqual("Tags not found");
  }
});

test("Should not find file if URI is not valid", async () => {
  mockingoose(Tags).toReturn(
    { _id: "507f191e810c19729de860ea", URI: "bob/goes/to/town.mp3" },
    "findOne"
  );
  try {
    await FilesUtil.openFile("507f191e810c19729de860ea");
  } catch (error) {
    expect(error.message).toEqual("File not found");
  }
});

test("Should return URI if file is found", async () => {
  mockingoose(Tags).toReturn(
    { _id: "507f191e810c19729de860ea", URI: "bob/goes/to/town.mp3" },
    "findOne"
  );
  mockfs({
    "bob/goes/to": {
      "town.mp3": "123456"
    }
  });
  const uri = await FilesUtil.openFile("507f191e810c19729de860ea");
  expect(uri).toEqual("bob/goes/to/town.mp3");
});

test("Should create file with URI based on genre/artist/album tags", async () => {
  const jsonTestTags = TagsUtil.extract(getTestTags());
  const expectedTags = TagsUtil.extract(getTestTags());
  const dirname = process.env.MUSIC_DIR + "/Alternative/bob-artist/bob-album";
  expectedTags.URI = dirname + "/town.mp3";

  TagsUtil.getTags = jest.fn();
  TagsUtil.getTags.mockResolvedValue(jsonTestTags);

  FilesUtilHelper.writeFileToDisk = jest.fn();
  FilesUtilHelper.writeTagsToDatabase = jest.fn(x => null);
  FilesUtilHelper.writeTagsToDatabase.mockResolvedValue(null);

  const file = { originalname: "town.mp3" };
  await FilesUtil.saveFile(file);

  expect(FilesUtilHelper.writeTagsToDatabase).toBeCalledWith(expectedTags);
  expect(FilesUtilHelper.writeFileToDisk).toBeCalledWith(
    file,
    dirname,
    expectedTags.URI
  );
});
