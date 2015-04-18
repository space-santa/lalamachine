#ifndef AUTOPLAYLISTMANAGER_H
#define AUTOPLAYLISTMANAGER_H

#include <QObject>
#include <QStringList>
#include <QJsonObject>
#include <QJsonArray>
#include "config.h"
#include "autoplaylistobject.h"

class AutoPlaylistManager : public QObject
{
    Q_OBJECT

    // clang-format off
    Q_PROPERTY(QStringList autoPlaylistNames
               READ getAutoPlaylistNames
               NOTIFY autoPlaylistNamesChanged)
    // clang-format on
public:
    explicit AutoPlaylistManager(QObject *parent = 0);
    ~AutoPlaylistManager();

    QStringList getAutoPlaylistNames();

    QJsonArray getAutoPlaylist(const QString name) const;
    void saveAutoPlaylist(const QString &name, const QJsonArray &args);
    void deleteAutoPlaylist(const QString &name);

signals:
    void autoPlaylistNamesChanged();

public slots:
private:
    void saveAutoPlaylist(const QString &name,
                          const QList<AutoPlaylistObject> &args) const;

    QString getPath(const QString &name) const;
};

#endif  // AUTOPLAYLISTMANAGER_H
