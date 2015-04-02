import QtQuick 2.0
import QtQuick.Dialogs 1.2
import QtQuick.Layouts 1.1

import Lala 1.0

Dialog {
    width: 500
    height: 250

    title: "Info"

    SysInfo {
        id: sysinfo
    }

    ColumnLayout {
        id: col
        height: 180
        property int childHeight: height / 6
        KeyValueText {
            labelText: "App Name: "
            valueText: sysinfo.appName()
            labelWidth: 150
            width: 500
            height: col.childHeight
        }
        KeyValueText {
            labelText: "Version: "
            valueText: sysinfo.appVersion()
            labelWidth: 150
            width: 500
            height: col.childHeight
        }
        KeyValueText {
            labelText: "Config directory: "
            valueText: sysinfo.lalaDir()
            labelWidth: 150
            width: 500
            height: col.childHeight
        }
        KeyValueText {
            labelText: "Your system: "
            valueText: sysinfo.prettyProductName()
            labelWidth: 150
            width: 500
            height: col.childHeight
        }
        KeyValueText {
            labelText: "License: "
            valueText: "GPL v3"
            labelWidth: 150
            width: 500
            height: col.childHeight
        }
        KeyValueText {
            labelText: "Copyright: "
            valueText: "2015 Armin Zirkel"
            labelWidth: 150
            width: 500
            height: col.childHeight
        }
    }
}

