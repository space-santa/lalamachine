# lalamachine

## Intro

lalamachine is just another music player. It features:

* A library with nice filter and search functionality.
* Playlists which can be exported into a folder.

## Dependencies

Qt5, LalaServer

## Build it

I only support Windows. This used to be multiplattform, and will probably work without too much adjustments on Linux but I simply don't have the time to support multiple targets.

### MSVC

The Qt tools for Visual Studio are required. Open the solution and press play. For that to work you need to copy all the required Qt *.ddl and folders into the appropriate folder (the one where lalamachine.exe is located, probably `release` or `debug`). There must also be a folder `LalaServer` that should contain `LalaServer.exe` and all its dependencies.

## Contribute

If you'd like to contribute to lalamachine,

* Test. Either by using the app or by adding to the qt unit test in ./test/lalatest
* Create packages for other operating systems.
* Write code and send me a pull request. [There is also a list of issues that wants to be resolved here.](https://github.com/space-santa/lalamachine/projects/1)
* Let me know how you like the app and what functionality is missing.

## License

Copyright 2015-2019 Claus Zirkel

Lalamachine is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Lalamachine is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with lalamachine.  If not, see <http://www.gnu.org/licenses/>.
