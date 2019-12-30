const mm = require("music-metadata");

const extract = json => {
  const tags = {
    title: json.common.title,
    artist: json.common.artist,
    album: json.common.album,
    comment: json.common.comment[0],
    track: json.common.track.no,
    genre: json.common.genre[0],
    disk: json.common.disk.no,
    year: json.common.year,
    bpm: json.common.bpm,
    key: json.common.key,
    duration: json.format.duration
  };

  return tags;
};

const getTags = file => {
  return new Promise(async resolve => {
    if (!file) {
      throw new Error("Please send a music file.");
    }
    try {
      const metadata = await mm.parseBuffer(file.buffer, "audio/mpeg", {
        duration: true
      });
      resolve(extract(metadata));
    } catch (error) {
      reject(error);
    }
  });
};

module.exports = { getTags, extract };
