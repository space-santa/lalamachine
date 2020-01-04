const express = require("express");
const router = new express.Router();

const DB = require("../db/interface");
const TitleFilter = require("../db/titlefilter");

router.get("/titles", async (req, res) => {
  const filter = new TitleFilter();
  filter.fromQuery(req.query);

  try {
    const titles = await DB.getFilteredTitles(filter);
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
