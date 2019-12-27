const genreLookup = require("id3-genre");

const extract = json => {
  const tags = {
    title: json.title,
    artist: json.artist,
    album: json.album,
    comment: json.comment.text ? json.comment.text : json.comment,
    track: json.track,
    genre:
      genreLookup(+json.genre) === "Unknown"
        ? json.genre
        : genreLookup(+json.genre),
    disc: json.TPOS ? json.TPOS.data : undefined,
    year: json.year ? json.year : json.TDRC ? json.TDRC.data : undefined,
    BPM: json.TBPM ? json.TBPM.data : undefined,
    KEY: json.TKEY ? json.TKEY.data : undefined
  };

  return tags;
};

module.exports = extract;
