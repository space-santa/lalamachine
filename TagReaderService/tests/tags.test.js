const request = require("supertest");
const app = require("../src/app");

test("Should get tags object when posting a file to /tags", async () => {
  const response = await request(app)
    .post("/tags")
    .attach("file", "tests/fixtures/good.mp3")
    .expect(200);
  console.log(response.body);
});
