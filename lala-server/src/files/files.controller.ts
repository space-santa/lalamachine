import { Controller, Get, Param, ParseIntPipe, Res } from "@nestjs/common";
import * as path from "path";

@Controller("files")
export class FilesController {
  @Get("/:id")
  getFileById(@Param("id", ParseIntPipe) id: number, @Res() res): Buffer {
    return res.sendFile(
      path.resolve(__dirname + "../../../test/fixtures/good.mp3")
    );
  }
}
