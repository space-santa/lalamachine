# lalamachine

## Intro

lalamachine is just another music player. It features:

* A library with nice filter and search functionality.
* Playlists which can be exported into a folder.
* Automatic playlists

## Fedora packages

I have a yum repository on gemfury. To use it, put this in `/etc/yum.repos.d/fury.repo`

```bash
[fury]
name=space-santa repo
baseurl=https://tdFaGYsdNpoXxsaukHct@yum.fury.io/space-santa/
enabled=1
gpgcheck=0
```

And then:

```bash
sudo dnf --refresh update
sudo dnf install lalamachine
```

This will also install `libsingleinstance`.

## Dependencies

qtbase, qtmultimedia, taglib, libsingleinstance

## Build it

Using the qmake of Qt5, the build and install process is very straightforward:

```bash
qmake
make
sudo make install
```

The software will be installed in /usr/bin.

To remove lalamachine,

```bash
sudo make uninstall
```

## Build it on Windows with mingw

This is a bit more involved.
Install [Qt](https://www.qt.io/download), [cmake](https://cmake.org/download/), and [git](https://git-scm.com/downloads)

(I am going to assume that there is a `D` drive.)

Open git bash

```
cd /d/
mkdir lalamachine
cd lalamachine
git clone git@github.com:space-santa/lalamachine.git
```

The result is `/d/lalamachine/lalamachine/`.
Next download [taglib](http://taglib.org/releases/taglib-1.11.1.tar.gz) and move it into `/d/lalamachine/`.

Also, add `D:\Qt\Tools\mingw530_32\bin` (or wherever your toolchain is) to the `PATH`.

Back in git bash

```
cd /d/lalamachine/
tar xzfp taglib-1.11.1.tar.gz
cd taglib-1.11.1
mkdir build
cd build
cmake -G "MinGW Makefiles" -D CMAKE_CXX_COMPILER=g++.exe -D CMAKE_MAKE_PROGRAM=mingw32-make.exe -DBUILD_SHARED_LIBS=ON -DENABLE_STATIC_RUNTIME=OFF ..
mingw32-make.exe
```

## Debug

On Fedora >22, don't forget to enable QDebug output. To do that, add or create

`~/.config/QtProject/qtlogging.ini` with this content:

```bash
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
* Write code and send me a pull request. [There is also a list of issues that wants to be resolved here.](https://github.com/space-santa/lalamachine/projects/1)
* Let me know how you like the app and what functionality is missing.

## License

Copyright 2015-2018 Claus Zirkel

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
