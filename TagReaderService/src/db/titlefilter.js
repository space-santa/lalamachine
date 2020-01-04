class TitleFilter {
  constructor() {
    this.title = "";
    this.genreId = "";
    this.artistId = "";
    this.albumId = "";
    this.sortBy = "title";
    this.sortAscending = true;
  }

  fromQuery(query) {
    if (query.title) {
      this.title = new RegExp(query.title);
    }

    if (query.genreId) {
      this.genreId = query.genreId;
    }

    if (query.artistId) {
      this.artistId = query.artistId;
    }

    if (query.albumId) {
      this.albumId = query.albumId;
    }

    if (query.sortBy) {
      const parts = query.sortBy.split(":");
      filter.sortBy = parts[0];
      filter.sortAscending = parts[1] === "asc";
    }
  }

  filterObject() {
    const filter = {};
    if (this.title) {
      filter.title = this.title;
    }
    if (this.genreId) {
      filter.genre = this.genreId;
    }
    if (this.artistId) {
      filter.artist = this.artistId;
    }
    if (this.albumId) {
      filter.album = this.albumId;
    }
    return filter;
  }

  sortObject() {
    const sort = {};
    sort[this.sortBy] = this.sortAscending ? 1 : -1;
    return sort;
  }
}

module.exports = TitleFilter;
