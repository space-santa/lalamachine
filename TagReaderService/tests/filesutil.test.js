const FilesUtil = require("../src/utils/filesutil");
require("../src/db/mongoose");

test("Should reject promise if no tags are found", async () => {
  try {
    await FilesUtil.openFile("5e0845d641d8d63018802b4f");
  } catch (error) {
    expect(error).toEqual("Tags not found");
  }
});
