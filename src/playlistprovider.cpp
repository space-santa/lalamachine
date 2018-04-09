// This file is part of lalamachine. License is GPL-3.0 (or later).

#include "playlistprovider.h"

#include <QDir>
#include <QJsonArray>
#include <QJsonDocument>
#include <QStandardPaths>

#include "config.h"

PlaylistProvider::PlaylistProvider(QQuickItem* parent) : QQuickItem(parent) {
    setPlaylistNames(getPlaylistNames());
    QDir dir(Config::PLAYLISTDIR);
    dir.mkpath(Config::PLAYLISTDIR);

    watcher_.addPath(Config::PLAYLISTDIR);
    connect(&watcher_, &QFileSystemWatcher::directoryChanged, this, &PlaylistProvider::handleDirChange);
}

// This requires explanation.
// Originally I had the playlistNames property just READ getPlaylistNames()
// without a write method. I emitted playlistNamesChanged in writePlaylist()
// and deletePlaylist. The signal emission in deletePlaylist would reliably
// segfault. I then added WRITE setPlaylistNames to the property and only
// emitted the signal there. Same endresult, writePlaylist fine, deletePlaylist
// segfault. So now I listen to directory changes and emit the signal then.
// No more segfaults.
void PlaylistProvider::handleDirChange() {
    setPlaylistNames(getPlaylistNames());
    watcher_.addPath(Config::PLAYLISTDIR);
}

void PlaylistProvider::writePlaylist(const QString& name, const QJsonArray& json) const {
    if (name.isEmpty()) {
        return;
    }

    QFile file(playlistPath(name));

    if (!file.open(QIODevice::WriteOnly | QIODevice::Text)) {
        qWarning() << "Couldn't write playlist" << name;
        return;
    }

    QTextStream out(&file);
    QJsonDocument doc(json);
    out << doc.toJson();
}

QJsonArray PlaylistProvider::readPlaylist(const QString& name) const {
    QFile file(playlistPath(name));

    if (!file.open(QIODevice::ReadOnly | QIODevice::Text)) {
        qWarning() << "Couldn't read playlist" << name;
        return QJsonArray();
    }

    // I ran into an interesting bug on Windows. A playlist had a track
    // with Umlauts. Lalamachine displays the track properly. It writes
    // the track properly into the playlist file.
    // But when reading the file,
    // QJsonDocument::fromJson(file.readAll(), &error)
    // tells me that it is not a UTF8 string. QTextStream saves the day.
    QTextStream in(&file);
    QJsonParseError error;
    QJsonDocument jd = QJsonDocument::fromJson(in.readAll().toUtf8(), &error);
    qDebug() << "readPlaylist QJsonParseError:" << error.errorString();

    return jd.array();
}

QStringList PlaylistProvider::getPlaylistNames() const {
    QDir d(Config::PLAYLISTDIR);
    QStringList filters;
    filters << "*.m3u";
    d.setNameFilters(filters);
    QStringList names;
    QFileInfoList entries = d.entryInfoList();

    for (int i = 0; i < entries.length(); ++i) {
        if (entries[i].baseName() != "cs1m090") {
            names.append(entries[i].baseName());
        }
    }

    return names;
}

QString PlaylistProvider::playlistPath(const QString& name) const {
    return Config::PLAYLISTDIR + "/" + name + ".m3u";
}

void PlaylistProvider::deletePlaylist(const QString& name) const {
    Q_ASSERT(QStringList(playlistNames()).contains(name));
    QFile::remove(playlistPath(name));
}

void PlaylistProvider::addTrackToNamedList(const QString& listName, const QJsonArray& track)
{
    Q_UNUSED(listName)
    Q_UNUSED(track)
    throw std::runtime_error("Implement PlaylistModel::addTrackToNamedList");
    // load the list
    // append the track
    // write the list
}

void PlaylistProvider::setPlaylistNames(const QStringList& list) {
    playlistNames_ = list;
    emit playlistNamesChanged();
}

QStringList PlaylistProvider::playlistNames() const {
    return playlistNames_;
}
