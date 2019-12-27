const request = require("supertest");
const app = require("../src/app");

test("Should get correct genre string from number when posting a file to /tags", async () => {
  const response = await request(app)
    .get("/tags")
    .attach("file", "tests/fixtures/good.mp3")
    .expect(200);
  console.log(response.body);
  expect(response.body.genre).toEqual("Alternative");
});

test("Should get correct genre string object when posting a file to /tags", async () => {
  const response = await request(app)
    .get("/tags")
    .attach("file", "tests/fixtures/genre.mp3")
    .expect(200);
  console.log(response.body);
  expect(response.body.genre).toEqual("Garage / Bassline / Grime");
});
