const express = require("express");
require("./db/mongoose");
const fileRouter = require("./router/files");
const musiclibRouter = require("./router/musiclib");

const app = express();

app.use(express.json());
app.use(fileRouter);
app.use(musiclibRouter);

module.exports = app;
