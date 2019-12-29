const request = require("supertest");
const app = require("../src/app");

test("Should get correct genre string from number when posting a file to /tags", async () => {
  const response = await request(app)
    .get("/tags")
    .attach("music", "tests/fixtures/good.mp3")
    .expect(200);
  expect(response.body.genre).toEqual("Alternative");
});

test("Should get correct genre string object when posting a file to /tags", async () => {
  const response = await request(app)
    .get("/tags")
    .attach("music", "tests/fixtures/genre.mp3")
    .expect(200);
  expect(response.body.genre).toEqual("Garage / Bassline / Grime");
});

test("Should get the discnumber if it has one", async () => {
  const response = await request(app)
    .get("/tags")
    .attach("music", "tests/fixtures/disc.mp3")
    .expect(200);
  expect(response.body.disc).toEqual("2");
});

const TagsUtil = require("../src/utils/tagsutil");

const testTags = {
  title: "bob title",
  artist: "bob artist",
  album: "bob album",
  comment: { text: "bob comment" },
  track: "1/19",
  genre: "20",
  TPOS: { data: "2/9" },
  TDRC: { data: "2005" },
  TBPM: { data: "123" },
  TKEY: { data: "7A" }
};

const getTestTags = () => {
  return JSON.parse(JSON.stringify(testTags));
};

test("Should get tags from testTags", async () => {
  const tags = TagsUtil.extract(getTestTags());
  expect(tags).toEqual({
    title: "bob title",
    artist: "bob artist",
    album: "bob album",
    comment: "bob comment",
    track: "1",
    genre: "Alternative",
    disc: "2",
    year: "2005",
    BPM: "123",
    KEY: "7A"
  });
});

test("Should get string comment", async () => {
  let localTestTags = getTestTags();
  localTestTags.comment = "new bob comment";
  const tags = TagsUtil.extract(localTestTags);
  expect(tags.comment).toEqual("new bob comment");
});

test("Should get year from year id3 tag", async () => {
  let localTestTags = getTestTags();
  delete localTestTags.TDRC;
  localTestTags.year = "1997";
  const tags = TagsUtil.extract(localTestTags);
  expect(tags.year).toEqual("1997");
});

test("Should not include year in tags without year or TDRC id3 tag", async () => {
  let localTestTags = getTestTags();
  delete localTestTags.TDRC;
  const tags = TagsUtil.extract(localTestTags);
  expect(tags.year).toBeUndefined();
});

test("Should not include BPM in tags without TBPM id3 tag", async () => {
  let localTestTags = getTestTags();
  delete localTestTags.TBPM;
  const tags = TagsUtil.extract(localTestTags);
  expect(tags.BPM).toBeUndefined();
});

test("Should not include KEY in tags without TKEY id3 tag", async () => {
  let localTestTags = getTestTags();
  delete localTestTags.TKEY;
  const tags = TagsUtil.extract(localTestTags);
  expect(tags.KEY).toBeUndefined();
});
