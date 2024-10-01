import { Track } from "./Track";

describe("Track", () => {
  it("should create instance", () => {
    const track = new Track();
    expect(track).toBeTruthy();
  });

  it("should create instance from valid json", () => {
    const track = Track.fromJson({
      id: 4,
      trackNumber: 3,
      discNumber: 2,
      title: "bob",
      length: 507,
      comment: "11A",
      album: { id: 3, name: "the bob album" },
      artist: { id: 77, name: "bob goes to town" },
      genre: { id: 1234, name: "barn metal" },
      year: 2014,
      url: "https://bob/goes/to/town.mp3",
      fileId: 4,
    });
    expect(track).toBeTruthy();
    expect(track.id).toEqual(4);
  });
});
