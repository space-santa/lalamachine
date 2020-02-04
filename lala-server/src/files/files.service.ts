import { Injectable } from "@nestjs/common";
import { ConfigService } from "@nestjs/config";
import * as path from "path";
import { parseBuffer } from "music-metadata";
import { mkdirSync, existsSync, writeFileSync, PathLike } from "fs";

@Injectable()
export class FilesService {
  constructor(private readonly configService: ConfigService) {}

  async saveFile(file: any) {
    const jsonTags = await this.getTags(file);
    const dirname = this.getDirname(
      jsonTags,
      this.configService.get<string>("MUSIC_DIR")
    );
    jsonTags.URI = this.getFilename(dirname, file.originalname);

    this.writeFileToDisk(file, dirname, jsonTags.URI);

    // await writeTagsToDatabase(jsonTags);
  }

  async saveFiles(files: any[]) {
    for (let file of files) {
      await this.saveFile(file);
    }
  }

  getFileUri(tagId: number) {
    return path.resolve(__dirname + "../../../test/fixtures/good.mp3");

    // const tags = await getTagsById(tagId);

    // try {
    //   openSync(tags.URI);
    // } catch (error) {
    //   await deleteTagsById(tagId);
    //   throw new Error("File not found");
    // }

    // return tags.URI;
  }

  extract(json) {
    const tags = {
      title: json.common.title,
      artist: json.common.artist,
      album: json.common.album,
      comment: json.common.comment[0],
      track: json.common.track.no,
      genre: json.common.genre[0],
      disk: json.common.disk.no,
      year: json.common.year,
      bpm: json.common.bpm,
      key: json.common.key,
      duration: json.format.duration,
      URI: ""
    };

    return tags;
  }

  async getTags(file) {
    if (!file) {
      throw new Error("Please send a music file.");
    }
    const metadata = await parseBuffer(file.buffer, "audio/mpeg", {
      duration: true
    });
    return this.extract(metadata);
  }

  private replaceWhitespaceWithUnderscore(value: string): string {
    return value.replace(/\s/g, "_");
  }

  private replaceNonWordWithDash(value: string): string {
    return value.replace(/\W/g, "-");
  }

  getDirname(tags, musicDir: string): string {
    const genre = tags.genre
      ? this.replaceNonWordWithDash(tags.genre)
      : "unknown";
    const artist = tags.artist
      ? this.replaceNonWordWithDash(tags.artist)
      : "unknown";
    const album = tags.album
      ? this.replaceNonWordWithDash(tags.album)
      : "unknown";
    const dirname = `${musicDir}/${genre}/${artist}/${album}`;
    return dirname;
  }

  getFilename(dirname: string, originalname: string): string {
    return dirname + "/" + this.replaceWhitespaceWithUnderscore(originalname);
  }

  writeFileToDisk(file, dirname: PathLike, fileUri: PathLike): void {
    mkdirSync(dirname, { recursive: true });
    if (existsSync(fileUri)) {
      return;
    }
    writeFileSync(fileUri, file.buffer);
  }
}
