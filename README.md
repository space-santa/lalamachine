# lalamachine

## Intro

lalamachine is just another music player. It features:

- A library with nice filter and search functionality.
- Playlists which can be exported into a folder.

## Dependencies

dotnet core 3.0

## Run it

Either install the .msi from the release section, or clone the repo and

```bash
cd Lalamachine
dotnet run
```

## Roadmap

The addition of TagReaderService marks the beginning of the end of the desktop app. The long term goal is to reinvent the web-app-music-player.
TagReaderService will evolve to be LalaServer. It will take care of all the things (which actually aren't that many). Things we need:

- LalaSPA
- LalaServer
  - MongoDB
  - TagReaderService
  - FileServer
  - FileUploadService

### Endpoints

- Fileupload POST /files/ ✔
  - File --> extract tags --> save tags to DB --> store file with UUID as filename (or, maybe make the URI `genre/artist/album/title` which would match the filters)
- Download GET /files/:id ✔
- GET /albums/ ✔
- GET /genres/ ✔
- GET /artists/ ✔
- GET /titles/ ✔

We leave auth for Ron.

## Contribute

If you'd like to contribute to lalamachine,

- Test. Either by using the app or by adding to the unit tests.
- Write code and create a pull request. [There is a list of issues that wants to be resolved.](https://github.com/space-santa/lalamachine/issues)
- Let me know how you like the app and what functionality is missing.

## Licence

Copyright 2015-2020 Claus Zirkel

Lalamachine is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Lalamachine is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with lalamachine. If not, see <http://www.gnu.org/licenses/>.
