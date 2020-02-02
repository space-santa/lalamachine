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
  @UseInterceptors(FilesInterceptor("files"))
  uploadFile(@UploadedFiles() files) {
    console.log(files);
  }
}
