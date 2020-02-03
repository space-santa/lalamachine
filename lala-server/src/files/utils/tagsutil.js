import { parseBuffer } from "music-metadata";

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

const getTags = async file => {
  if (!file) {
    throw new Error("Please send a music file.");
  }
  const metadata = await parseBuffer(file.buffer, "audio/mpeg", {
    duration: true
  });
  return extract(metadata);
};

export default { getTags, extract };
