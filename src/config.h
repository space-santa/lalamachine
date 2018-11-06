/*
Copyright 2015-2018 Claus Zirkel

This file is part of lalamachine. License is GPL-3 or later.
*/

#pragma once

#include <QJsonArray>
#include <QJsonObject>
#include <QObject>
#include <QSettings>
#include <QString>
#include <QUrl>

class Config : public QObject {
    Q_OBJECT

    Q_PROPERTY(double volume READ volume WRITE setVolume NOTIFY volumeChanged)

    Q_PROPERTY(QUrl libPath READ libPath WRITE setLibPath NOTIFY libPathChanged)

    Q_PROPERTY(QString lastPlaylist READ lastPlaylist WRITE setLastPlaylist NOTIFY lastPlaylistChanged)

    Q_PROPERTY(QJsonArray playlistColumns READ playlistColumns WRITE setPlaylistColumns NOTIFY playlistColumnsChanged)

public:
    explicit Config(QObject* parent = 0);

    static const QString LALADIR;
    static const QString CONFIGPATH;
    static const QString LOGPATH;
    static const QString PLAYLISTDIR;
    static const QString MUSICLIBDB;
    static const QString MAINDBNAME;
    static const QString SCANNERDBNAME;

	static const QString execPath();

    void loadConfig();

    void setVolume(double val);
    double volume();

    void setPlaylistColumns(const QJsonArray& list);
    QJsonArray playlistColumns();

    void setLibPath(const QUrl& path);
    QUrl libPath() const;

    void setLastPlaylist(const QString& name);
    QString lastPlaylist() const;

    static QJsonObject loadJsonFile(const QString& path);
    static void saveJsonFile(const QString& path, const QJsonObject& obj);
    static void ensureDir(const QString& path);
    static QJsonObject addKey(const QString& key);

signals:
    void volumeChanged();
    void libPathChanged();
    void lastPlaylistChanged();
    void playlistColumnsChanged();

private:
    QSettings settings_;

    void migrateVolume(const QJsonObject& config);
    void migratePlaylistColumns(const QJsonObject& config);
    void migrateLibPath(const QJsonObject& config);
    void migrateLastPlaylist(const QJsonObject& config);
};
