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

#ifndef MUSICLIBSCANNER_H
#define MUSICLIBSCANNER_H

#include <QThread>
#include <QJsonObject>
#include <QString>
#include <QQuickItem>
#include <QMutex>

class MusicLibScanner : public QObject
{
    Q_OBJECT

public:
    MusicLibScanner(QObject *parent = 0);

public slots:
    void scanLib(const QString &path);

signals:
    void scanStarted();
    void scanComplete(const QJsonObject &lib);

private:
    QMutex mutex_;
    bool suffixCheck(const QString &val) const;
};

class MusicLib : public QQuickItem
{
    Q_OBJECT

    Q_PROPERTY(QJsonObject musicLib
               READ musicLib
               NOTIFY musicLibChanged)

    Q_PROPERTY(bool scanning
               READ scanning
               WRITE setScanning
               NOTIFY scanningChanged)

    Q_PROPERTY(QJsonObject displayLib
               READ displayLib
               NOTIFY displayLibChanged)

    Q_PROPERTY(QString libPath
               READ libPath
               WRITE setLibPath
               NOTIFY libPathChanged)

    Q_PROPERTY(QString genreFilter
               READ genreFilter
               WRITE setGenreFilter
               NOTIFY genreFilterChanged)

    Q_PROPERTY(QString artistFilter
               READ artistFilter
               WRITE setArtistFilter
               NOTIFY artistFilterChanged)

    Q_PROPERTY(QString albumFilter
               READ albumFilter
               WRITE setAlbumFilter
               NOTIFY albumFilterChanged)

    Q_PROPERTY(QStringList genreList
               READ genreList
               NOTIFY genreListChanged)

    Q_PROPERTY(QStringList artistList
               READ artistList
               NOTIFY artistListChanged)

    Q_PROPERTY(QStringList albumList
               READ albumList
               NOTIFY albumListChanged)

public:
    MusicLib(QQuickItem *parent = 0);
    ~MusicLib();

    static const QString ALL_FILTER;

    QJsonObject musicLib() const;

    bool scanning() const;
    void setScanning(bool val);

    QJsonObject displayLib() const;

    QString libPath() const;
    void setLibPath(const QString &path);

    QString genreFilter() const;
    void setGenreFilter(const QString &val);

    QString artistFilter() const;
    void setArtistFilter(const QString &val);

    QString albumFilter() const;
    void setAlbumFilter(const QString &val);

    QStringList genreList() const;
    QStringList artistList() const;
    QStringList albumList() const;

    Q_INVOKABLE void rescan();

public slots:
    void scanFinished(const QJsonObject &lib);

signals:
    void startScan(const QString &path);
    void musicLibChanged();

    void scanningChanged();

    void displayLibChanged();

    void libPathChanged();
    void genreFilterChanged();
    void artistFilterChanged();
    void albumFilterChanged();

    void genreListChanged();
    void artistListChanged();
    void albumListChanged();

private:
    // scanner_ MUST be a raw pointer. When this is moved to a new thread, that
    // QThread becomes the parent. When the parent dies so does the child.
    // If this is then not a raw pointer a double free happens, because
    // the thread and this is trying to destroy the scanner.
    MusicLibScanner *scanner_ {new MusicLibScanner()};
    QThread scannerThread_ {};

    QJsonObject lib_ {};
    bool scanning_ {false};
    QJsonObject displayLib_ {};
    QString genreFilter_ {""};
    QString artistFilter_ {""};
    QString albumFilter_ {""};
    QString libPath_ {""};
    QStringList genreList_ {};
    QStringList artistList_ {};
    QStringList albumList_ {};

    bool checkVal(const QString &check, const QString &val) const;

    QStringList getList(const QString &what) const;

private slots:
    void readLibFile();
    void writeLibFile();

    void debugSignal();

    void setDisplayLib();
    void setGenreList();
    void setArtistList();
    void setAlbumList();

    void scanStarted();
};

#endif // MUSICLIB_H
