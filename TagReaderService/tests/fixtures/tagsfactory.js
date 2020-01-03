const testTags = {
  common: {
    title: "bob title",
    artist: "bob artist",
    album: "bob album",
    comment: ["bob comment"],
    track: "1/19",
    genre: ["Alternative"],
    disk: { no: 2 },
    track: { no: 3 },
    year: 2005,
    bpm: 123,
    key: "7A"
  },
  format: {
    duration: 234.56
  }
};

const getTestTags = () => {
  return JSON.parse(JSON.stringify(testTags));
};

module.exports = getTestTags;
