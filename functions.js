.pragma library

function millisToMinSec(millis) {
    var min = Math.floor(millis / 1000 / 60).toString()
    var rem = Math.floor(millis / 1000 % 60).toString()
    rem = rem.length === 1 ? "0" + rem : rem
    return min + ":" + rem
}
