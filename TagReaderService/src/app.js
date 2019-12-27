const express = require("express");
const multer = require("multer");
const jsmediatags = require("jsmediatags");

const extract = require("./tags");

const app = express();

app.use(express.json());

const upload = multer({
  limits: { fileSize: 20 * 1000 * 1000 },
  fileFilter(req, file, cb) {
    if (!file.originalname.toLowerCase().match(/\.(mp3|m4a)$/)) {
      return cb(new Error("Please upload music."));
    }

    cb(undefined, true);
  }
});

app.get(
  "/tags",
  upload.single("music"),
  async (req, res) => {
    if (!req.file) {
      return res.status(400).send("Please send a music file.");
    }
    jsmediatags.read(req.file.buffer, {
      onSuccess: function(tag) {
        res.send(extract(tag.tags));
      },
      onError: function(error) {
        res.status(400).send(":(", error.type, error.info);
      }
    });
  },
  (error, req, res, next) => {
    res.status(400).send({ error: error.message });
  }
);

module.exports = app;
