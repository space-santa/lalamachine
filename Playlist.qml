import QtQuick 2.0
import QtQuick.Controls 1.2

Rectangle {
    width: 100
    height: 62

    ListModel {
        id: playlist_model
    }

    TableView {
        id: playlist_view
        model: playlist_model
    }
}
