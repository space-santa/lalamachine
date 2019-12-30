const mongoose = require("mongoose");

const tagsSchema = new mongoose.Schema(
  {
    track: { type: Number, default: 1, required: true, trim: true },
    disk: { type: Number, default: 1, required: true, trim: true },
    title: { type: String, required: true, trim: true },
    duration: { type: Number, required: true },
    comment: { type: String, required: false, trim: true },
    album: {
      type: mongoose.Schema.Types.ObjectId,
      required: false,
      ref: "Album"
    },
    artist: {
      type: mongoose.Schema.Types.ObjectId,
      required: false,
      ref: "Artist"
    },
    genre: {
      type: mongoose.Schema.Types.ObjectId,
      required: false,
      ref: "Genre"
    },
    year: { type: Number, required: false, trim: true },
    bpm: { type: Number, required: false, trim: true },
    key: { type: String, required: false, trim: true },
    URI: { type: String, required: true }
  },
  {
    timestamps: true
  }
);

const Tags = mongoose.model("Tags", tagsSchema);

module.exports = Tags;
