const FilesUtilHelper = require("../src/utils/filesutilhelper");

test("Should replace whitespace with underscore", () => {
  const dirname = "/bob";
  const filename = "goes to town.mp3";
  expect(FilesUtilHelper.getFilename(dirname, filename)).toEqual(
    "/bob/goes_to_town.mp3"
  );
});

test('Should use "unknown" if genre, artist, or album is undefined', () => {
  expect(FilesUtilHelper.getDirname({})).toEqual(
    `${process.env.MUSIC_DIR}/unknown/unknown/unknown`
  );
});

test("Should create dirname based on genre/artist/album", () => {
  expect(
    FilesUtilHelper.getDirname({
      genre: "bob",
      artist: "goes to",
      album: "town"
    })
  ).toEqual(`${process.env.MUSIC_DIR}/bob/goes-to/town`);
});
