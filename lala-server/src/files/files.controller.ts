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
import * as path from "path";

@Controller("files")
export class FilesController {
  @Get("/:id")
  getFileById(@Param("id", ParseIntPipe) id: number, @Res() res): Buffer {
    return res.sendFile(
      path.resolve(__dirname + "../../../test/fixtures/good.mp3")
    );
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
  uploadFile(@UploadedFiles() files) {
    console.log(files);
  }
}
