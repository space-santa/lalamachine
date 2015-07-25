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
    Q_ENUMS(PlaylistRoles)

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

    Q_INVOKABLE void sortRole(int role, Qt::SortOrder order = Qt::AscendingOrder);

private:
    QList<Track> list_;

    void append(Track track);
    static bool sortTrackAsc(Track t1, Track t2);
    static bool sortTrackDesc(Track t1, Track t2);
    static bool sortTitleAsc(Track t1, Track t2);
    static bool sortTitleDesc(Track t1, Track t2);
    static bool sortGenreAsc(Track t1, Track t2);
    static bool sortGenreDesc(Track t1, Track t2);
    static bool sortArtistAsc(Track t1, Track t2);
    static bool sortArtistDesc(Track t1, Track t2);
    static bool sortAlbumAsc(Track t1, Track t2);
    static bool sortAlbumDesc(Track t1, Track t2);
    static bool sortCommentAsc(Track t1, Track t2);
    static bool sortCommentDesc(Track t1, Track t2);
    static int compareComment(QString c1, QString c2);

    static bool sortLengthAsc(Track t1, Track t2);
    static bool sortLengthDesc(Track t1, Track t2);
    static bool sortYearAsc(Track t1, Track t2);
    static bool sortYearDesc(Track t1, Track t2);
    static bool sortDateAddedAsc(Track t1, Track t2);
    static bool sortDateAddedDesc(Track t1, Track t2);
};

#endif  // PLAYLISTMODEL_H
