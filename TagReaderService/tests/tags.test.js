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
  expect(response.body.disc).toEqual("2/2");
});

const extract = require("../src/tags");

const testTags = {
  title: "bob title",
  artist: "bob artist",
  album: "bob album",
  comment: {
    text: "bob comment"
  },
  track: "1/19",
  genre: "20",
  TPOS: {
    data: "2/9"
  },
  year: "2005",
  TBPM: {
    data: "123"
  },
  TKEY: {
    data: "7A"
  }
};

test("Should get tags from testTags", async () => {
  const tags = extract(testTags);
  expect(tags).toEqual({
    title: "bob title",
    artist: "bob artist",
    album: "bob album",
    comment: "bob comment",
    track: "1/19",
    genre: "Alternative",
    disc: "2/9",
    year: "2005",
    BPM: "123",
    KEY: "7A"
  });
});
