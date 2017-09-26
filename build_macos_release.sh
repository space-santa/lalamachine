#!/bin/bash

cd release
qmake CONFIG+=release ../lalamachine.pro
make -j3
cp /usr/local/lib/libtag.dylib lalamachine.app/Contents/MacOS/
