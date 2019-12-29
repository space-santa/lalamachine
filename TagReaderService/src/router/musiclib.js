const express = require("express");
const router = new express.Router();

const Tags = require("../models/tags");

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
    const genres = await Tags.find({}, "genre");
    return res.send(genres);
  } catch (error) {
    return res.status(500).send({ error });
  }
});

module.exports = router;
