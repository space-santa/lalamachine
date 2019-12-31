const mockfs = require("mock-fs");

const mockingoose = require("mockingoose").default;
const Tags = require("../src/models/tags");
const FilesUtil = require("../src/utils/filesutil");

test("Should not find tags if there are no tags", async () => {
  mockingoose(Tags).toReturn(undefined, "findOne");
  try {
    await FilesUtil.openFile("507f191e810c19729de860ea");
  } catch (error) {
    expect(error).toEqual("Tags not found");
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
    expect(error).toEqual("File not found");
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
