const express = require("express");
const multer = require("multer");

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
  try {
    // do the thing to get the tags
    res.send({ tags: "lotsa tags" });
  } catch (error) {
    res.status(404).send();
  }
});

module.exports = app;
