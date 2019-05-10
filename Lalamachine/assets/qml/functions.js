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

// Random number within range min <= num <= max.
function randomInt(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min
}

function numToString(num) {
    var retval = "";

    if (num < 10) {
        retval += "0";
    }

    retval += num;

    return retval;
}

function secondsToDisplayString(seconds) {
    var minInSec = 60;
    var hourInSec = 60 * minInSec;
    var dayInSec = 24 * hourInSec;

    var totalSec = seconds;
    // Integer division to get the days.
    var days = Math.floor(seconds / dayInSec);
    // We have the days, now we only want the rest.
    seconds -= days * dayInSec;
    // Integer division to get the hours.
    var hours = Math.floor(seconds / hourInSec);
    // Remove the hours.
    seconds -= hours * hourInSec;
    // Get the rest.
    var mins = Math.floor(seconds / minInSec);
    var sec = seconds - mins * minInSec;

    var retVal = "";

    if (days > 0) {
        retVal += days;
        if (days === 1) {
            retVal += " day, ";
        } else {
            retVal += " days, ";
        }
    }

    if (days > 0 || hours > 0) {
        retVal += numToString(hours);
        retVal += ":";
    }

    retVal += numToString(mins);
    retVal += ":";
    retVal += numToString(sec);

    return retVal;
}
