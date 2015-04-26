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

