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
    friend class TestAutoPlaylist;
    Q_OBJECT

    Q_PROPERTY(QJsonObject list READ list WRITE setList NOTIFY listChanged)

public:
    explicit AutoPlaylist(const QString &name, QObject *parent = 0);
    ~AutoPlaylist();

    void load();
    void save();

    void addApo(const AutoPlaylistObject &apo);
    void clear();

    QJsonObject list();
    void setList(const QJsonObject &list);

    // The list of traccks for the player.
    QJsonArray tracklist();

signals:
    void listChanged();

public slots:

private:
    QString name_;
    QList<AutoPlaylistObject> apos_;

    // The JSON representation of this autoplaylist (e.g. for saving).
    QJsonArray toJson();

    QString toQuery();
};

#endif // AUTOPLAYLIST_H
