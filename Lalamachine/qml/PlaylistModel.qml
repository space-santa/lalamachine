import QtQuick 2.0
import QtQuick.Controls 1.2

ListModel {
    id: playlist_model
    property int totalPlaytime: 0

    function setLibrary(json){
        for (var i = 0; i < json.length; ++i) {
            append(json[i]);
        }
        updateTotalLength()
    }

    function compare(a, b) {
        if (a.title < b.title)
            return -1;
        if (a.title > b.title)
            return 1;
        return 0;
    }

    function sortRole(role, order) {
        console.log(role, order)
        var list = []

        for (var i = 0; i < playlist_model.count; ++i) {
            list.push(playlist_model.get(i))
        }
        list = list.sort(compare);
        fromJson(list, true)
        // TODO: Implement this
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
        let tmp = {}
        tmp["album"] = rowData.album
        tmp["artist"] = rowData.artistString
        tmp["genre"] = rowData.genreString
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
