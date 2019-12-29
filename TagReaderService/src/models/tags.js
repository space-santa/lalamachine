const mongoose = require("mongoose");

const tagsSchema = new mongoose.Schema(
  {
    title: { type: String, required: true, trim: true },
    artist: { type: String, required: false, trim: true },
    album: { type: String, required: false, trim: true },
    comment: { type: String, required: false, trim: true },
    track: { type: Number, default: 1, required: true, trim: true },
    genre: { type: String, required: false, trim: true },
    disc: { type: Number, default: 1, required: true, trim: true },
    year: { type: Number, required: false, trim: true },
    BPM: { type: Number, required: false, trim: true },
    KEY: { type: String, required: false, trim: true },
    URI: { type: String, required: true }
  },
  {
    timestamps: true
  }
);

const Tags = mongoose.model("Tags", tagsSchema);

module.exports = Tags;
