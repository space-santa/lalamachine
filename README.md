# lalamachine

## Intro

lalamachine is a rhythmbox clone. I like rhythmbox a lot, but it is missing
certain features. On the other hand it has so many
features I never use. It is also the only Gnome application I use which adds dependencies to my system.  
And since I enjoy software development I decided to write lalamachine.

## Dependencies

lalamachine is a Qt5 application and uses QtMultimedia to play music.
The only other (direct) dependency is taglib.  
Since QtMultimedia uses GStreamer as backend you probably want to install
GStreamer codecs.

## Build it

Using the qmake of Qt5, the build and install process is very straightforward:


```
#!bash

qmake
make
sudo make install
```

The software will be installed in /opt/rmean/bin for now.  
This will change to /usr/bin eventually, at latest for version 1.0

To remove lalamachine,


```
#!bash

sudo make uninstall
```


## Package

I build packages for openSUSE 13.2 and Tumbleweed with the openSUSE build service.  
The download repositories are here:

* [openSUSE13.2](http://download.opensuse.org/repositories/home:/rmean/openSUSE_13.2/)
* [Tumbleweed](http://download.opensuse.org/repositories/home:/rmean/openSUSE_Tumbleweed/)

## Contribute

I would appreciate any help. If you'd like to contribute to lalamachine,

* Test. Either by using the app or by adding to the qt unit test in ./test/lalatest
* Create packages for other distributions.
* Add to the wiki.
* Write code and send me a pull request. There is also a list of issues that wants to be resolved.
* Let me know how you like the app and what functionality is missing.


## License

Copyright 2015 Armin Zirkel

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