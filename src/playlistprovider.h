// This file is part of lalamachine. License is GPL-3.0 (or later).

#pragma once

#include <QFileSystemWatcher>
#include <QJsonArray>
#include <QQuickItem>

class PlaylistProvider : public QQuickItem {
    Q_OBJECT

    Q_PROPERTY(QStringList playlistNames READ playlistNames WRITE setPlaylistNames NOTIFY playlistNamesChanged)

public:
    explicit PlaylistProvider(QQuickItem* parent = 0);

    Q_INVOKABLE void writePlaylist(const QString& name, const QJsonArray& json) const;
    Q_INVOKABLE QJsonArray readPlaylist(const QString& name) const;
    QStringList getPlaylistNames() const;
    Q_INVOKABLE QString playlistPath(const QString& name) const;
    Q_INVOKABLE void deletePlaylist(const QString& name) const;
    Q_INVOKABLE void addTracksToNamedPlaylist(const QString& listName, const QJsonArray& tracks);

    void setPlaylistNames(const QStringList& list);
    QStringList playlistNames() const;

signals:
    void playlistNamesChanged();

private slots:
    void handleDirChange();

private:
    QStringList playlistNames_;
    QFileSystemWatcher watcher_;
};
