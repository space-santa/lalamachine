#ifndef PLAYLISTMODEL_H
#define PLAYLISTMODEL_H

#include <QObject>
#include <QVariant>
#include <QAbstractListModel>

#include "track.h"

class PlaylistModel : public QAbstractListModel
{
    Q_OBJECT
public:
    enum PlaylistRoles {
        TrackRole,
        TitleRole,
        CommentRole,
        GenreRole,
        ArtistRole,
        AlbumRole,
        LengthRole,
        LengthStringRole,
        MrlRole,
        PathRole,
        YearRole,
        DateAddedRole,
        IdRole
    };

    explicit PlaylistModel(QObject *parent = 0);

    QHash<int, QByteArray> roleNames() const;
    int rowCount(const QModelIndex &parent = QModelIndex()) const;
    QVariant data(const QModelIndex &index, int role = Qt::DisplayRole) const;
    bool setData(const QModelIndex &index,
                 const QVariant &value,
                 int role = Qt::EditRole);
    Qt::ItemFlags flags(const QModelIndex &index) const;

    Q_INVOKABLE int count() const;
    Q_INVOKABLE void move(int from, int to);
    Q_INVOKABLE void remove(int row);
    Q_INVOKABLE QJsonObject get(int row);
    Q_INVOKABLE void clear();
    Q_INVOKABLE void append(const QJsonObject &json);

private:
    QList<Track> list_;

    void append(Track track);
};

#endif  // PLAYLISTMODEL_H
