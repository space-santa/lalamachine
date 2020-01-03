const mockingoose = require("mockingoose");
const request = require("supertest");
const app = require("../src/app");

test("Should do nothing at all", async () => {
  expect(1).toEqual(1);
});
