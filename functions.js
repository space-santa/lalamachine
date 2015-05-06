/*
Copyright 2015 Armin Zirkel

This file is part of lalamachine.

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
*/
function millisToSec(millis) {
    return Math.round(millis / 1000)
}

function millisToMinSec(millis) {
    return secToMinSec(millisToSec(millis))
}

function secToMinSec(sec) {
    var min = Math.floor(sec / 60).toString()
    var rem = Math.floor(sec % 60).toString()
    rem = rem.length === 1 ? "0" + rem : rem
    return min + ":" + rem
}

function getSafeValue(value) {
    if (typeof (value) === "undefined") {
        return ""
    } else {
        return value
    }
}

// TODO: why is it necessary to check the mrl?
function checkMrl(mrl) {
    mrl = mrl.toString()
    if (mrl.indexOf("file://") != "0") {
        mrl = "file://" + mrl
    }
    return mrl
}

// Random number within range min <= num <= max.
function randomInt(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min
}
