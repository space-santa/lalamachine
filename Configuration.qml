import QtQuick 2.0
import Lala 1.0

Item {
    property alias volume: json.volume

    function saveConfig() {
        json.saveConfig()
    }

    Config {
        id: json

        onVolumeChanged: console.log("Volume changed to", json.volume)
    }
}
