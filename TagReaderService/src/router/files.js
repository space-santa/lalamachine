const express = require("express");
const router = new express.Router();

const multer = require("multer");

const TagsUtil = require("../utils/tagsutil");
const FilesUtil = require("../utils/filesutil");

const upload = multer({
  limits: { fileSize: 20 * 1000 * 1000 },
  fileFilter(req, file, cb) {
    if (!file.originalname.toLowerCase().match(/\.(mp3|m4a)$/)) {
      return cb(new Error("Please upload music."));
    }

    cb(undefined, true);
  }
});

router.get(
  "/tags",
  upload.single("music"),
  async (req, res) => {
    try {
      const tags = await TagsUtil.getTags(req.file);
      return res.send(tags);
    } catch (error) {
      return res.status(400).send({ error: error.message });
    }
  },
  (error, req, res, next) => {
    res.status(400).send({ error: error.message });
  }
);

router.post(
  "/files",
  upload.single("music"),
  async (req, res) => {
    if (!req.file) {
      return res.status(400).send("Please send a music file.");
    }

    try {
      await FilesUtil.saveFile(req.file);
      res.send();
    } catch (error) {
      res.status(400).send({ error: error.message });
    }
  },
  (error, req, res, next) => {
    res.status(400).send({ error: error.message });
  }
);

router.get("/files/:id", async (req, res) => {
  try {
    const file = await FilesUtil.openFile(req.params.id);
    res.sendFile(file);
  } catch (error) {
    res.status(500).send({ error });
  }
});

module.exports = router;
