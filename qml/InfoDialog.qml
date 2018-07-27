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
import QtQuick 2.0
import QtQuick.Dialogs 1.2
import QtQuick.Layouts 1.1

import Lala 1.0

Dialog {
    width: mainWidth + 8
    height: 250

    readonly property int mainWidth: 700
    readonly property int mainLabelWidth: 200

    title: "Info"

    SysInfo {
        id: sysinfo
    }

    contentItem: Rectangle {
        color: "darkblue"
        border.color: "darkorange"
        border.width: 4

        ColumnLayout {
            id: col
            height: 180
            anchors.centerIn: parent
            property int childHeight: height / 6
            KeyValueText {
                labelText: "App Name: "
                valueText: sysinfo.appName()
                labelWidth: mainLabelWidth
                width: mainWidth
                height: col.childHeight
            }
            KeyValueText {
                labelText: "Version: "
                valueText: sysinfo.appVersion()
                labelWidth: mainLabelWidth
                width: mainWidth
                height: col.childHeight
            }
            KeyValueText {
                labelText: "Config directory: "
                valueText: sysinfo.lalaDir()
                labelWidth: mainLabelWidth
                width: mainWidth
                height: col.childHeight
            }
            KeyValueText {
                labelText: "License: "
                valueText: "GPL v3"
                labelWidth: mainLabelWidth
                width: mainWidth
                height: col.childHeight
            }
            KeyValueText {
                labelText: "Copyright: "
                valueText: "2015-2018 Claus Zirkel"
                labelWidth: mainLabelWidth
                width: mainWidth
                height: col.childHeight
            }
        }
    }
}
