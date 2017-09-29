# lalamachine

## Intro

lalamachine is just another music player. It features:

* A library with nice filter and search functionality.
* Playlists which can be exported into a folder.
* Automatic playlists

## Dependencies

lalamachine is a Qt5 application.
The only other (direct) dependency is taglib.

## Build it

Using the qmake of Qt5, the build and install process is very straightforward:


```
#!bash

qmake
make
sudo make install
```

The software will be installed in /usr/bin.

To remove lalamachine,


```
#!bash

sudo make uninstall
```


## Debug

On Fedora >22, don't forget to enable QDebug output. To do that, add or create

`~/.config/QtProject/qtlogging.ini` with this content:

```
[Rules]
*.debug=true
qt.*.debug=false
```

The first rule enables debug output. The second line disables the Qt internal one.

## Contribute

I would appreciate any help. If you'd like to contribute to lalamachine,

* Test. Either by using the app or by adding to the qt unit test in ./test/lalatest
* Create packages for other distributions.
* Add to the wiki.
* Write code and send me a pull request. There is also a list of issues that wants to be resolved. [Trello-board](https://trello.com/b/7C3MEK7M/lalamachine)
* Let me know how you like the app and what functionality is missing.


## License

Copyright 2015-2017 Claus Zirkel

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
