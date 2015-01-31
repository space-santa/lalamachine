.pragma library

function millisToSec(millis) {
    return Math.floor(millis / 1000)
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
