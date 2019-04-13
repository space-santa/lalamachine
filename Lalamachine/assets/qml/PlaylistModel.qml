import QtQuick 2.0
import QtQuick.Controls 1.2

ListModel {
    id: playlist_model
    property int totalPlaytime: 0

    function setLibrary(json){
        for (var i = 0; i < json.length; ++i) {
            appendTags(json[i]);
        }
        updateTotalLength()
    }

    function appendTags(json) {
        // It is abit tricky to get data in and out of the model.
        // This is why we have to only append objects with keys that we want.
        // In particular, artist and artistString (and the genre equivalent) would cause problems.
        let tmp = rowDataToTrackJson(json)
        append(tmp)
    }

    function createCompare(what, order) {
        if (what.length < 1) {
            return function(a, b) {
                if (a["album"] == b["album"] && a["discNumber"] == b["discNumber"] && a["track"] == b["track"])
                    return 0;
                if (a["album"] == b["album"] && a["discNumber"] == b["discNumber"] && a["track"] < b["track"])
                    return -1;
                if (a["album"] == b["album"] && a["discNumber"] < b["discNumber"])
                    return -1;
                if (a["album"] < b["album"])
                    return -1;
                return 1;
            }
        }

        let how = order === 0 ? 1 : -1
        return function(a, b) {
            if (a[what] < b[what])
                return -1 * how;
            if (a[what] > b[what])
                return 1 * how;
            return 0;
        }
    }

    function defaultSort() {
        sortRole("", "");
    }

    function sortRole(role, order) {
        console.log("sortRole:", role, order)
        var list = toJson()
        list = list.sort(createCompare(role, order));
        fromJson(list, true)
    }

    function toJson() {
        let retval = []

        for (let i = 0; i < playlist_model.count; ++i) {
            retval.push(getTrackJsonAtIndex(i));
        }

        return retval;
    }

    function getTrackJsonAtIndex(index) {
        return rowDataToTrackJson(playlist_model.get(index));
    }

    function rowDataToTrackJson(rowData) {
        // This function looks absolutely redundant.
        // Why not just pass in the original object, right?
        // The problem is that we must not have other keys, and for some reason `delete`ing them didn't work.
        let tmp = {}
        tmp["album"] = rowData.album
        tmp["artistString"] = rowData.artistString
        tmp["genreString"] = rowData.genreString
        tmp["comment"] = rowData.comment
        tmp["track"] = rowData.track
        tmp["title"] = rowData.title
        tmp["mrl"] = rowData.mrl
        tmp["path"] = rowData.path
        tmp["length"] = rowData.length
        tmp["lengthString"] = rowData.lengthString
        tmp["year"] = rowData.year
        tmp["discNumber"] = rowData.discNumber
        tmp["id"] = rowData.id
        return tmp
    }

    function fromJson(json, clearList) {
        if (clearList) {
            clear();
        }
        setLibrary(json);
    }

    function updateTotalLength() {
        let totalLength = 0;

        for (let i = 0; i < playlist_model.count; ++i) {
            totalLength += getTrackJsonAtIndex(i).length;
        }

        totalPlaytime = totalLength
    }
}
