isEmpty(target.path) {
    target.path = /usr/bin
    export(target.path)
}
INSTALLS += target

icon.files = images/lalamachine-new.png
export(icon.files)
isEmpty(icon.path) {
    icon.path = /usr/share/icons/hicolor/64x64/apps
    export(icon.path)
}

INSTALLS += icon

desktop.files = lalamachine.desktop
export(desktop.files)
isEmpty(desktop.path) {
    desktop.path = /usr/share/applications
    export(desktop.path)
}

INSTALLS += desktop

export(INSTALLS)
