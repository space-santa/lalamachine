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

#include "config.h"

#include <QByteArray>
#include <QDebug>
#include <QDir>
#include <QFile>
#include <QJsonArray>
#include <QJsonDocument>
#include <QJsonObject>
#include <QJsonValue>
#include <QStandardPaths>
#include <QUrl>
#include <QVariant>

const QString Config::LALADIR =
    QStandardPaths::writableLocation(QStandardPaths::GenericConfigLocation) + "/lalamachine";
const QString Config::CONFIGPATH = Config::LALADIR + "/config.json";
const QString Config::AUTOPLAYLISTDIR = Config::LALADIR + "/autoplaylists";
const QString Config::PLAYLISTDIR = Config::LALADIR + "/playlists";
const QString Config::MUSICLIBDB = Config::LALADIR + "/musiclib.sq3";

Config::Config(QObject* parent) : QObject(parent) {
    QDir dir(Config::LALADIR);
    dir.mkpath(Config::LALADIR);
    loadConfig();
}

void Config::saveConfig() {
    saveJsonFile(Config::CONFIGPATH, config_);
}

void Config::loadConfig() {
    config_ = loadJsonFile(Config::CONFIGPATH);
}

void Config::setVolume(double val) {
    config_.insert("volume", val);
    emit volumeChanged();
}

double Config::volume() {
    // I initialize the volume with 0.4. I turns out to be confusing for users
    // that on first start the app starts silent (as in volume = 0).
    double retval = 0.4;
    QJsonValue v = config_.value("volume");

    if (v.isDouble()) {
        retval = v.toDouble();
    } else {
        qWarning("No proper volume defined in config.json");
    }

    qDebug() << "Volume loaded" << retval;
    return retval;
}

void Config::setPlaylistColumns(const QJsonArray& list) {
    config_.insert("playlistColumns", list);
    emit playlistColumnsChanged();
}

QJsonObject Config::addKey(const QString& key) {
    QJsonObject o;
    o.insert("key", key);
    o.insert("value", "true");
    return o;
}

QJsonArray Config::playlistColumns() {
    QJsonArray retval;
    QJsonValue v = config_.value("playlistColumns");

    if (v.isArray()) {
        retval = v.toArray();
    } else {
        qWarning("No playlist columns defined.");
    }

    bool hasTrack = false;
    bool hasTitle = false;
    bool hasComment = false;
    bool hasGenre = false;
    bool hasArtist = false;
    bool hasAlbum = false;
    bool hasLength = false;
    bool hasYear = false;
    bool hasDate = false;
    bool hasDisc = false;

    for (int i = 0; i < retval.count(); ++i) {
        QString key = retval[i].toObject().value("key").toString();
        if (key == "track") {
            hasTrack = true;
        } else if (key == "title") {
            hasTitle = true;
        } else if (key == "comment") {
            hasComment = true;
        } else if (key == "genre") {
            hasGenre = true;
        } else if (key == "artist") {
            hasArtist = true;
        } else if (key == "album") {
            hasAlbum = true;
        } else if (key == "length") {
            hasLength = true;
        } else if (key == "year") {
            hasYear = true;
        } else if (key == "dateAdded") {
            hasDate = true;
        } else if (key == "discNumber") {
            hasDisc = true;
        }
    }

    if (!hasTrack) { retval.append(addKey("track")); }

    if (!hasTitle) { retval.append(addKey("title")); }

    if (!hasComment) { retval.append(addKey("comment")); }

    if (!hasGenre) { retval.append(addKey("genre")); }

    if (!hasArtist) { retval.append(addKey("artist")); }

    if (!hasAlbum) { retval.append(addKey("album")); }

    if (!hasLength) { retval.append(addKey("length")); }

    if (!hasYear) { retval.append(addKey("year")); }

    if (!hasDisc) { retval.append(addKey("discNumber")); }

    if (!hasDate) { retval.append(addKey("dateAdded")); }

    config_.insert("playlistColumns", retval);
    qDebug() << "playlistColumns loaded" << retval;
    return retval;
}

void Config::setLibPath(const QUrl& path) {
    QString actualPath = path.toLocalFile();
    Q_ASSERT(QDir(actualPath).exists());
    config_.insert("libPath", actualPath);
    // We want to save the file right after setting the path because we want
    // config.json to match musiclib.json as soon as it changes.
    saveConfig();
    emit libPathChanged();
}

QUrl Config::libPath() const {
    QUrl retval;
    QJsonValue v = config_.value("libPath");

    if (v.isString()) {
        retval = QUrl(v.toString());

        if (!QDir(retval.path()).exists()) {
            retval.clear();
            qCritical() << "libPath doesn't exist!";
        }
    } else {
        qWarning("No proper libPath defined in config.json");
    }

    return retval;
}

void Config::setLastPlaylist(const QString& name) {
    config_.insert("lastPlaylist", name);
    emit lastPlaylistChanged();
}

QString Config::lastPlaylist() const {
    QString retval("");
    QJsonValue v = config_.value("lastPlaylist");

    if (v.isString()) { retval = v.toString(); }

    return retval;
}

QJsonObject Config::loadJsonFile(const QString& path) {
    // Reading the JSON
    QFile loadFile(path);

    if (!loadFile.open(QIODevice::ReadOnly)) {
        qWarning("Couldn't open json file.");
        return QJsonObject();
    }

    QByteArray val;
    val = loadFile.readAll();
    // Create the JsonDocument
    QJsonDocument d = QJsonDocument::fromJson(val);

    if (!d.isObject() || d.isEmpty() || d.isNull()) {
        qWarning("Invalid config file.");
        return QJsonObject();
    } else {
        return d.object();
    }
}

void Config::saveJsonFile(const QString& path, const QJsonObject& obj) {
    QJsonDocument d(obj);
    QFile saveFile(path);
    qDebug() << "Config::setData";

    ensureDir(path);

    if (!saveFile.open(QIODevice::WriteOnly)) {
        qWarning("Couldn't open config file.");
        return;
    }

    // If we want the utf8 encoding to be correctly written to the file,
    // we cannot write the JSON document directly. We must write an encoded
    // QString into the file.
    QTextStream out(&saveFile);
    out << QString::fromUtf8(d.toJson());
    qDebug() << "Config::dataSet";
}

void Config::ensureDir(const QString& path) {
    QFileInfo fi(path);
    QDir dir(fi.absoluteDir());

    if (!dir.exists()) { dir.mkpath(fi.absolutePath()); }
}
