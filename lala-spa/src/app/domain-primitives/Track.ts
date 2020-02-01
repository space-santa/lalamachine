import { NameWithId } from "./NameWithId";
import { Year } from "./Year";
import { Id } from "./Id";
import { TrackNumber } from "./TrackNumber";
import { DiscNumber } from "./DiscNumber";
import { Name } from "./Name";
import { Length } from "./Length";
import { MediaUrl } from "./MediaUrl";

export class Track {
  private _id: Id;
  private _trackNumber: TrackNumber;
  private _discNumber: DiscNumber;
  private _title: Name;
  private _length: Length;
  private _comment: Name;
  private _album: NameWithId;
  private _artist: NameWithId;
  private _genre: NameWithId;
  private _year: Year;
  private _fileId: Id;

  static fromJson(json: any): Track {
    let track = new Track();
    track._id = new Id(json["id"]);
    track._trackNumber = new TrackNumber(json["trackNumber"]);
    track._discNumber = new DiscNumber(json["discNumber"]);
    track._title = new Name(json["title"]);
    track._length = new Length(json["length"]);
    track._comment = new Name(json["comment"]);
    track._album = NameWithId.fromJson(json["album"]);
    track._artist = NameWithId.fromJson(json["artist"]);
    track._genre = NameWithId.fromJson(json["genre"]);
    track._year = new Year(json["year"]);
    track._fileId = new Id(json["fileId"]);

    return track;
  }

  static fakeTrack(): Track {
    let track = new Track();

    track._id = new Id(5);
    track._trackNumber = new TrackNumber(1);
    track._discNumber = new DiscNumber(1);
    track._title = new Name("Bob goes to town");
    track._length = new Length(234);
    track._comment = new Name("11A");
    track._album = new NameWithId(3, "Bob's greatest hits.");
    track._artist = new NameWithId(4, "The Bob Band");
    track._genre = new NameWithId(5, "BobMetal");
    track._year = new Year(2001);
    track._fileId = new Id(4);

    return track;
  }

  get id(): number {
    return this._id.value;
  }

  get trackNumber(): number {
    return this._trackNumber.value;
  }

  get discNumber(): number {
    return this._discNumber.value;
  }

  get title(): string {
    return this._title.value;
  }

  get length(): string {
    return this._length.asString;
  }

  get comment(): string {
    return this._comment.value;
  }

  get genreName(): string {
    return this._genre.name;
  }

  get artistName(): string {
    return this._artist.name;
  }

  get albumName(): string {
    return this._album.name;
  }

  get year(): number {
    return this._year.value;
  }

  get fileId(): number {
    return this._fileId.value;
  }
}
