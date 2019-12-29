const mongoose = require("mongoose");

const albumSchema = new mongoose.Schema(
  {
    name: { type: String, required: true, trim: true },
    artist: {
      type: mongoose.Schema.Types.ObjectId,
      required: true,
      ref: "Artist"
    },
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

albumSchema.virtual("tags", {
  ref: "Tags",
  localField: "_id",
  foreignField: "album"
});

const Album = mongoose.model("Album", albumSchema);

module.exports = Album;
