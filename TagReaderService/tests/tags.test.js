const mockingoose = require("mockingoose").default;
const request = require("supertest");
const app = require("../src/app");
const getTestTags = require("./fixtures/tagsfactory");
const TagsUtil = require("../src/utils/tagsutil");

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
  expect(response.body.disk).toEqual(2);
});

test("Should get tags from testTags", async () => {
  const tags = TagsUtil.extract(getTestTags());
  expect(tags).toEqual({
    title: "bob title",
    artist: "bob artist",
    album: "bob album",
    comment: "bob comment",
    track: 3,
    genre: "Alternative",
    disk: 2,
    year: 2005,
    bpm: 123,
    key: "7A",
    duration: 234.56
  });
});
