const mongoose = require("mongoose");

const genreSchema = new mongoose.Schema(
  {
    name: { type: String, required: true, trim: true, unique: true }
  },
  {
    timestamps: true
  }
);

genreSchema.virtual("tags", {
  ref: "Tags",
  localField: "_id",
  foreignField: "genre"
});

genreSchema.virtual("artist", {
  ref: "Artist",
  localField: "_id",
  foreignField: "genre"
});

genreSchema.virtual("album", {
  ref: "Album",
  localField: "_id",
  foreignField: "genre"
});

const Genre = mongoose.model("Genre", genreSchema);

module.exports = Genre;
