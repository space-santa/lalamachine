import QtQuick 2.0
import QtQuick.Dialogs 1.2
import QtQuick.Controls 1.2
import QtQuick.Layouts 1.1

Dialog {
    width: 500
    height: 400

    KeyValueTableView {
        id: playlist_columns
        height: 200
        width: 200
        keyTitle: "Tag"
        valueTitle: "visible"

        list: [{
                key: "track",
                value: "true"
            }, {
                key: "title",
                value: "true"
            }, {
                key: "comment",
                value: "true"
            }, {
                key: "genre",
                value: "true"
            }, {
                key: "artist",
                value: "true"
            }, {
                key: "album",
                value: "true"
            }, {
                key: "length",
                value: "true"
            }, {
                key: "year",
                value: "true"
            }]
    }
}
