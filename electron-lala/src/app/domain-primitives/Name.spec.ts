import { Name } from "./Name";

describe("Name", () => {
  it("should create a Name object", () => {
    const name = new Name("bob");
    expect(name.value).toEqual("bob");
  });

  it("should throw a range error when string is empty", () => {
    expect(function() {
      new Name("");
    }).toThrowError(RangeError);
  });

  it("should throw a range error when string is only whitespace", () => {
    expect(function() {
      new Name("    ");
    }).toThrowError(RangeError);
  });

  it("should throw a range error when string is too long", () => {
    let name = "B";
    for (let i = 0; i < 100; ++i) {
      name += "o";
    }
    expect(function() {
      new Name(name);
    }).toThrowError(RangeError);
  });
});
