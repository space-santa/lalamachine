module.exports = {
  openSync() {},
  mkdirSync() {},
  existsSync() {
    return false;
  },
  writeFileSync() {}
};
