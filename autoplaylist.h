#ifndef AUTOPLAYLIST_H
#define AUTOPLAYLIST_H

#include <QObject>
#include <QList>
#include <QJsonArray>

#include "autoplaylistobject.h"

// This class manages one auto playlist.
// It contains a list of AutoPlaylistObjects and can save/load the list.
class AutoPlaylist : public QObject
{
    Q_OBJECT

public:
    explicit AutoPlaylist(QObject *parent = 0);
    ~AutoPlaylist();

    void load();
    void save();

    void addApo();
    void removeApo();

    void clear();

    // The list of traccks for the player.
    QJsonArray tracklist();

signals:

public slots:

private:
    QString name_;
    QList<AutoPlaylistObject> apos_;

    // The JSON representation of this autoplaylist (e.g. for saving).
    QJsonArray toJson();
};

#endif // AUTOPLAYLIST_H
