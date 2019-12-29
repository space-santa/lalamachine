const express = require("express");
require("./db/mongoose");
const fileRouter = require("./router/files");

const app = express();

app.use(express.json());
app.use(fileRouter);

module.exports = app;
