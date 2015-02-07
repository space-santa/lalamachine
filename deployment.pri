isEmpty(target.path) {
    target.path = /opt/rmean/bin
    export(target.path)
}
INSTALLS += target

icon.files = images/lalamachine.png
export(icon.files)
isEmpty(icon.path) {
    icon.path = /opt/rmean/icons
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
