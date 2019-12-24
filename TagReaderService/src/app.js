const express = require("express");
const multer = require("multer");
const jsmediatags = require("jsmediatags");

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

app.post("/tags", upload.single("file"), async (req, res) => {
  // TODO: Filter the tags to be more useful.
  jsmediatags.read(req.file.buffer, {
    onSuccess: function(tag) {
      res.send(tag);
    },
    onError: function(error) {
      res.status(404).send(":(", error.type, error.info);
    }
  });
});

module.exports = app;
