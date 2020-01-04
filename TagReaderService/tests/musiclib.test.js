const mockingoose = require("mockingoose").default;
const request = require("supertest");
const app = require("../src/app");

const Tags = require("../src/db/models/tags");
const Genre = require("../src/db/models/genre");
const Artist = require("../src/db/models/artist");
const Album = require("../src/db/models/album");

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

const albumList = [
  {
    _id: "507f191e810c19729de8aaea",
    name: "Black Album",
    artist: { _id: "507f191e810c19729de860ea", name: "Bobo" }
  },
  {
    _id: "507f191e810c19729de860ea",
    name: "White Album",
    artist: { _id: "507f191e810c19729de860ea", name: "Joe" }
  },
  {
    _id: "507f191e810c19729de860ff",
    name: "Green Red",
    artist: { _id: "507f191e810c19729de860ea", name: "Johnny" }
  }
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

test("Should get all artists", async () => {
  const finderMock = query => {
    return findNameMatch(query, artistList);
  };

  mockingoose(Artist).toReturn(finderMock, "find");
  const response = await request(app)
    .get("/artists")
    .expect(200);

  expect(response.body[0].name).toEqual("Johnny");
  expect(response.body[1].name).toEqual("Bobo");
  expect(response.body[2].name).toEqual("Boba the builder");
});

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

test("Should get all genres", async () => {
  const finderMock = query => {
    return findNameMatch(query, genreList);
  };

  mockingoose(Genre).toReturn(finderMock, "find");
  const response = await request(app)
    .get("/genres")
    .expect(200);

  expect(response.body[0].name).toEqual("Metal");
  expect(response.body[1].name).toEqual("Black Metal");
  expect(response.body[2].name).toEqual("Alternative");
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

test("Should get all albums", async () => {
  const finderMock = query => {
    return findNameMatch(query, albumList);
  };

  Album.schema.path("artist", Object);
  mockingoose(Album).toReturn(finderMock, "find");
  const response = await request(app)
    .get("/albums")
    .expect(200);

  expect(response.body).toEqual(albumList);
});

test("Should get albums by name", async () => {
  const finderMock = query => {
    return findNameMatch(query, albumList);
  };

  Album.schema.path("artist", Object);
  mockingoose(Album).toReturn(finderMock, "find");
  const response = await request(app)
    .get("/albums")
    .query({ name: "lbum" })
    .expect(200);

  expect(response.body[0].artist.name).toEqual("Bobo");
  expect(response.body[1].artist.name).toEqual("Joe");
});

test("Should find no album by name", async () => {
  const finderMock = query => {
    return findNameMatch(query, albumList);
  };

  Album.schema.path("artist", Object);
  mockingoose(Album).toReturn(finderMock, "find");
  const response = await request(app)
    .get("/albums")
    .query({ name: "Jim" })
    .expect(200);

  expect(response.body).toEqual([]);
});
