import { Controller, Get, Param, ParseIntPipe, Res } from "@nestjs/common";

@Controller("files")
export class FilesController {
  @Get("/:id")
  getFileById(@Param("id", ParseIntPipe) id: number, @Res() res): Buffer {
    return res.sendFile(
      "D:/code/lalamachine/lala-server/test/fixtures/good.mp3"
    );
  }
}
