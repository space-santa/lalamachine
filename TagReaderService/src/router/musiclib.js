const express = require("express");
const router = new express.Router();

const DB = require("../db/interface");

const Tags = require("../db/models/tags");

router.get("/titles", async (req, res) => {
  const filter = {};
  const sort = {};

  if (req.query.title) {
    filter.title = new RegExp(req.query.title);
  }

  if (req.query.genreId) {
    filter.genre = req.query.genreId;
  }

  if (req.query.artistId) {
    filter.artist = req.query.artistId;
  }

  if (req.query.albumId) {
    filter.album = req.query.albumId;
  }

  if (req.query.sortBy) {
    const parts = req.query.sortBy.split(":");
    sort[parts[0]] = parts[1] === "desc" ? -1 : 1;
  }

  try {
    const titles = await Tags.find(
      filter,
      "track disk title duration comment album artist genre year"
    )
      .populate({
        path: "genre",
        select: "name"
      })
      .populate({
        path: "artist",
        select: "name"
      })
      .populate({
        path: "album",
        select: "name"
      });
    return res.send(titles);
  } catch (error) {
    return res.status(500).send({ error });
  }
});

router.get("/genres", async (req, res) => {
  try {
    const genres = await DB.getFilteredGenres(req.query.name);
    return res.send(genres);
  } catch (error) {
    return res.status(500).send({ error });
  }
});

router.get("/artists", async (req, res) => {
  try {
    const artists = await DB.getFilteredArtists(req.query.name);
    return res.send(artists);
  } catch (error) {
    return res.status(500).send({ error });
  }
});

router.get("/albums", async (req, res) => {
  try {
    const albums = await DB.getFilteredAlbums(req.query.name);
    return res.send(albums);
  } catch (error) {
    return res.status(500).send({ error });
  }
});

module.exports = router;
