import { NameWithId } from "./NameWithId";
import { ValueIsUndefinedError } from "./ValueIsUndefinedError";

describe("NameWithId", () => {
  it("should create a NameWithId object from JSON", () => {
    let nameWid = NameWithId.fromJson({ id: 4, name: "bob" });
    expect(nameWid.id).toEqual(4);
    expect(nameWid.name).toEqual("bob");
  });

  it("should throw ValueIsUndefinedError when JSON has no id key", () => {
    expect(function () {
      NameWithId.fromJson({ name: "bob" });
    }).toThrowError(ValueIsUndefinedError);
  });

  it("should throw ValueIsUndefinedError when JSON has no name key", () => {
    expect(function () {
      NameWithId.fromJson({ id: 4 });
    }).toThrowError(ValueIsUndefinedError);
  });
});
