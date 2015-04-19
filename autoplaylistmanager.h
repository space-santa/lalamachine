#ifndef AUTOPLAYLISTMANAGER_H
#define AUTOPLAYLISTMANAGER_H

#include <QObject>
#include <QStringList>
#include <QJsonObject>
#include <QJsonArray>
#include <QFileSystemWatcher>
#include "config.h"
#include "autoplaylistobject.h"

class AutoPlaylistManager : public QObject
{
    Q_OBJECT

    // clang-format off
    Q_PROPERTY(QStringList autoPlaylistNames
               READ autoPlaylistNames
               WRITE setAutoPlaylistNames
               NOTIFY autoPlaylistNamesChanged)
    // clang-format on
public:
    explicit AutoPlaylistManager(QObject *parent = 0);
    ~AutoPlaylistManager();

    QStringList autoPlaylistNames();
    void setAutoPlaylistNames(const QStringList &names);

    QJsonArray getAutoPlaylist(const QString name) const;
    Q_INVOKABLE void saveAutoPlaylist(const QString &name,
                                      const QJsonArray &args);
    Q_INVOKABLE void deleteAutoPlaylist(const QString &name);

signals:
    void autoPlaylistNamesChanged();

public slots:

private slots:
    void handleDirChange();

private:
    QStringList autoPlaylistNames_{};
    QFileSystemWatcher watcher_{};
    void saveAutoPlaylist(const QString &name,
                          const QList<AutoPlaylistObject> &args) const;

    QString getPath(const QString &name) const;
    QStringList getPlaylistNames() const;
};

#endif  // AUTOPLAYLISTMANAGER_H
