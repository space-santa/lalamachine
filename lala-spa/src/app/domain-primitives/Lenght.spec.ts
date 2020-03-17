import { Length } from "./Length";

describe("Length", () => {
  it("should create a Length object", () => {
    const length = new Length(97);
    expect(length.seconds).toEqual(97);
  });

  it("should show a leading zero when seconds is one digit", () => {
    const length = new Length(67);
    expect(length.asString).toEqual("1:07");
  });

  it("should show a leading zero when minutes is one digit and hour is greater 0", () => {
    const length = new Length(3667);
    expect(length.asString).toEqual("1:01:07");
  });

  it("should throw a range error when seconds is 0", () => {
    expect(function() {
      new Length(0);
    }).toThrowError(RangeError);
  });

  it("should throw a range error when seconds is negative", () => {
    expect(function() {
      new Length(-10);
    }).toThrowError(RangeError);
  });

  it("should throw a range error when seconds is too big", () => {
    expect(function() {
      new Length(36001);
    }).toThrowError(RangeError);
  });
});
