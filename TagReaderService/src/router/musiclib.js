const express = require("express");
const router = new express.Router();

const Tags = require("../models/tags");
const Genre = require("../models/genre");
const Artist = require("../models/artist");
const Album = require("../models/album");

router.get("/titles", async (req, res) => {
  const filter = {};
  const sort = {};

  if (req.query.title) {
    filter.title = new RegExp(req.query.title);
  }

  if (req.query.genre) {
    filter.genre = req.query.genreId;
  }

  if (req.query.artist) {
    filter.artist = req.query.artistId;
  }

  if (req.query.album) {
    filter.album = req.query.albumId;
  }

  if (req.query.sortBy) {
    const parts = req.query.sortBy.split(":");
    sort[parts[0]] = parts[1] === "desc" ? -1 : 1;
  }

  try {
    const titles = await Tags.find(filter);
    return res.send(titles);
  } catch (error) {
    return res.status(500).send({ error });
  }
});

router.get("/genres", async (req, res) => {
  const filter = {};

  if (req.query.name) {
    filter.name = new RegExp(req.query.name);
  }
  console.log(filter);
  try {
    const genres = await Genre.find(filter, "name");
    return res.send(genres);
  } catch (error) {
    return res.status(500).send({ error });
  }
});

router.get("/artists", async (req, res) => {
  const filter = {};

  if (req.query.name) {
    filter.name = new RegExp(req.query.name);
  }

  try {
    const artists = await Artist.find(filter, "name");
    return res.send(artists);
  } catch (error) {
    return res.status(500).send({ error });
  }
});

router.get("/albums", async (req, res) => {
  const filter = {};

  if (req.query.name) {
    filter.name = new RegExp(req.query.name);
  }

  try {
    const albums = await Album.find(filter, "name");
    return res.send(albums);
  } catch (error) {
    return res.status(500).send({ error });
  }
});

module.exports = router;
