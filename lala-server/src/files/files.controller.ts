import {
  Controller,
  Get,
  Param,
  ParseIntPipe,
  Res,
  Post,
  UseInterceptors,
  UploadedFiles
} from "@nestjs/common";
import { FilesInterceptor } from "@nestjs/platform-express";
import { FilesService } from "./files.service";

@Controller("files")
export class FilesController {
  constructor(private filesService: FilesService) {}

  @Get("/:tagId")
  getFileById(@Param("tagId", ParseIntPipe) tagId: number, @Res() res): Buffer {
    return res.sendFile(this.filesService.getFileUri(tagId));
  }

  @Post()
  @UseInterceptors(
    FilesInterceptor("files", 999, {
      limits: { fileSize: 20 * 1000 * 1000 },
      fileFilter(req, file, cb) {
        if (!file.originalname.toLowerCase().match(/\.(mp3|m4a)$/)) {
          return cb(new Error("Please upload music."), false);
        }

        cb(undefined, true);
      }
    })
  )
  async uploadFile(@UploadedFiles() files) {
    return await this.filesService.saveFiles(files);
  }
}
