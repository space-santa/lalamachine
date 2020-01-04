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

    if (req.query.artistId) {
      this.artistId = query.artistId;
    }

    if (req.query.albumId) {
      this.albumId = query.albumId;
    }

    if (req.query.sortBy) {
      const parts = query.sortBy.split(":");
      filter.sortBy = parts[0];
      filter.sortAscending = parts[1] === "asc";
    }
  }

  filterObject() {
    return {
      title: this.title,
      genre: this.genreId,
      artist: this.artistId,
      album: this.artistId
    };
  }

  sortObject() {
    const sort = {};
    sort[this.sortBy] = this.sortAscending ? 1 : -1;
    return sort;
  }
}

module.exports = TitleFilter;
