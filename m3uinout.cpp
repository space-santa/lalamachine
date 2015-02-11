#include "m3uinout.h"

#include <QStandardPaths>
#include <QDir>

#include "config.h"

M3uInOut::M3uInOut(QQuickItem *parent) :
    QQuickItem(parent)
{
    qDebug() <<  m3uPath("test");
    QDir dir(Config::PLAYLISTDIR);
    dir.mkpath(Config::PLAYLISTDIR);
}

void M3uInOut::writePlaylist(const QString &name,
                             const QStringList files) const
{
    QFile file(m3uPath(name));

    if (file.open(QIODevice::WriteOnly | QIODevice::Text)) {
        QTextStream out(&file);

        for (int i = 0; i < files.length(); ++i) {
            out << files[i];

            if (!files[i].endsWith("\n")) {
                out << "\n";
            }
        }
    }
}

QStringList M3uInOut::readPlaylist(const QString &name) const
{
    QFile file(m3uPath(name));
    QStringList retVal;

    if (file.open(QIODevice::ReadOnly | QIODevice::Text)) {
        QTextStream in(&file);

        while (!in.atEnd()) {
            retVal.append("file://" + in.readLine());
        }
    }

    return retVal;
}

QStringList M3uInOut::getPlaylistNames() const
{
    QDir d(Config::PLAYLISTDIR);
    QStringList filters;
    filters << "*.m3u";
    d.setNameFilters(filters);
    return d.entryList();
}

QString M3uInOut::m3uPath(const QString &name) const
{
    return Config::PLAYLISTDIR + "/" + name + ".m3u";
}
