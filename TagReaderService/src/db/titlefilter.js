class TitleFilter {
  constructor() {
    this.title = "";
    this.genreId = "";
    this.artistId = "";
    this.albumId = "";
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
}

module.exports = TitleFilter;
