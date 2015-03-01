.pragma library

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
    if (typeof(value) === "undefined") {
        return ""
    } else {
        return value
    }
}

// TODO: why is it necessary to check the mrl?
function checkMrl(mrl) {
    mrl = mrl.toString()
    console.log("checkMrl pre", mrl)
    if (mrl.indexOf("file://") != "0") {
        mrl = "file://" + mrl
    }
    console.log("checkMrl post", mrl)
    return mrl
}
