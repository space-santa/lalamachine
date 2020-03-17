import { MediaUrl } from "./MediaUrl";
import { SuffixError } from "./SuffixError";
import { InsecureUrlError } from "./InsecureUrlError";

describe("MediaUrl", () => {
  it("should create object for mp3", () => {
    const testUrl = "https://bob/goes/to/town.mp3";
    const url = new MediaUrl(testUrl);
    expect(url.value).toEqual(testUrl);
  });

  it("should create object for m4a", () => {
    const testUrl = "https://bob/goes/to/town.m4a";
    const url = new MediaUrl(testUrl);
    expect(url.value).toEqual(testUrl);
  });

  it("should throw an error when url is not https", () => {
    expect(function() {
      new MediaUrl("http://bob/goes/to/town.mp3");
    }).toThrowError(InsecureUrlError);
  });

  it("should throw an error when suffix is not mp3 or m4a", () => {
    expect(function() {
      new MediaUrl("https://bob/goes/to/town.mbo");
    }).toThrowError(SuffixError);
  });
});
