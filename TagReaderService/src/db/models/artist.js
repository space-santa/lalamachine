const mongoose = require("mongoose");

const artistSchema = new mongoose.Schema(
  {
    name: { type: String, required: true, trim: true, unique: true },
    genre: {
      type: mongoose.Schema.Types.ObjectId,
      required: false,
      ref: "Genre"
    }
  },
  {
    timestamps: true
  }
);

artistSchema.virtual("tags", {
  ref: "Tags",
  localField: "_id",
  foreignField: "artist"
});

artistSchema.virtual("album", {
  ref: "Album",
  localField: "_id",
  foreignField: "artist"
});

const Artist = mongoose.model("Artist", artistSchema);

module.exports = Artist;
