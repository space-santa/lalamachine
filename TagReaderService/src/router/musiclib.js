const express = require("express");
const router = new express.Router();

const Tags = require("../models/tags");
const Genre = require("../models/genre");
const Artist = require("../models/artist");
const Album = require("../models/album");

router.get("/titles", async (req, res) => {
  try {
    const titles = await Tags.find({});
    return res.send(titles);
  } catch (error) {
    return res.status(500).send({ error });
  }
});

router.get("/genres", async (req, res) => {
  try {
    const genres = await Genre.find({});
    return res.send(genres);
  } catch (error) {
    return res.status(500).send({ error });
  }
});

router.get("/artists", async (req, res) => {
  try {
    const artists = await Artist.find({});
    return res.send(artists);
  } catch (error) {
    return res.status(500).send({ error });
  }
});

router.get("/albums", async (req, res) => {
  try {
    const albums = await Album.find({});
    return res.send(albums);
  } catch (error) {
    return res.status(500).send({ error });
  }
});

module.exports = router;
