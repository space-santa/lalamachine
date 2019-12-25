const genreLookup = require("id3-genre");

const extract = json => {
  console.log(json);
  const tags = {
    title: json.title,
    artist: json.artist,
    album: json.album,
    comment: json.comment.text,
    track: json.track,
    genre: genreLookup(+json.genre),
    disc: json.TPOS.data,
    BPM: json.TBPM.data,
    KEY: json.TKEY.data,
    year: json.TDRC.data
  };

  return tags;
};

module.exports = extract;
