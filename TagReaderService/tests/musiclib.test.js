const mockingoose = require("mockingoose").default;
const request = require("supertest");
const app = require("../src/app");

const Tags = require("../src/models/tags");
const Genre = require("../src/models/genre");
const Artist = require("../src/models/artist");
const Album = require("../src/models/album");

const artistList = [
  { _id: "507f191e810c19729de8aaea", name: "Johnny" },
  { _id: "507f191e810c19729de860ea", name: "Bobo" },
  { _id: "507f191e810c19729de860ff", name: "Boba the builder" }
];

const genreList = [
  { _id: "507f191e810c19729de8aaea", name: "Metal" },
  { _id: "507f191e810c19729de860ea", name: "Black Metal" },
  { _id: "507f191e810c19729de860ff", name: "Alternative" }
];

const findNameMatch = (query, list) => {
  const values = [];

  for (let item of list) {
    if (item.name.match(query.getQuery().name)) {
      values.push(item);
    }
  }

  return values;
};

test("Should get artists by name", async () => {
  const finderMock = query => {
    return findNameMatch(query, artistList);
  };

  mockingoose(Artist).toReturn(finderMock, "find");
  const response = await request(app)
    .get("/artists")
    .query({ name: "Bob" })
    .expect(200);

  expect(response.body[0].name).toEqual("Bobo");
  expect(response.body[1].name).toEqual("Boba the builder");
});

test("Should find no artists by name", async () => {
  const finderMock = query => {
    return findNameMatch(query, artistList);
  };

  mockingoose(Artist).toReturn(finderMock, "find");
  const response = await request(app)
    .get("/artists")
    .query({ name: "Jim" })
    .expect(200);

  expect(response.body).toEqual([]);
});

test("Should get genre by name", async () => {
  const finderMock = query => {
    return findNameMatch(query, genreList);
  };

  mockingoose(Genre).toReturn(finderMock, "find");
  const response = await request(app)
    .get("/genres")
    .query({ name: "etal" })
    .expect(200);

  expect(response.body[0].name).toEqual("Metal");
  expect(response.body[1].name).toEqual("Black Metal");
});

test("Should find no genre by name", async () => {
  const finderMock = query => {
    return findNameMatch(query, genreList);
  };

  mockingoose(Genre).toReturn(finderMock, "find");
  const response = await request(app)
    .get("/genres")
    .query({ name: "Jim" })
    .expect(200);

  expect(response.body).toEqual([]);
});
