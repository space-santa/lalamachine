#ifndef PLAYLISTMODEL_H
#define PLAYLISTMODEL_H

#include <QObject>
#include <QAbstractListModel>

#include "track.h"

class PlaylistModel : public QAbstractListModel
{
    Q_OBJECT
public:
    enum PlaylistRoles {
        TRACK = Qt::UserRole + 1,
        TITLE,
        COMMENT,
        GENRE,
        ARTIST,
        ALBUM,
        LENGTH,
        LENGTHSTRING,
        MRL,
        PATH,
        YEAR,
        DATEADDED,
        ID
    };

    explicit PlaylistModel(QObject *parent = 0);

    QHash<int, QByteArray> roleNames() const;
    int rowCount(const QModelIndex &parent = QModelIndex()) const;
    QVariant data(const QModelIndex &index, int role = Qt::DisplayRole) const;
    bool setData(const QModelIndex &index,
                 const QVariant &value,
                 int role = TRACK);
    Qt::ItemFlags flags(const QModelIndex &index) const;

    void append(Track track);

    Q_INVOKABLE int count() const;
    Q_INVOKABLE void move(int from, int to);
    Q_INVOKABLE void remove(int row);
    Q_INVOKABLE QJsonObject get(int row);
    Q_INVOKABLE void clear();
    Q_INVOKABLE void append(const QJsonObject &json);

signals:
    void beginInsertRows(const QModelIndex &parent, int first, int last);
    void endInsertRows();
    void beginRemoveRows(const QModelIndex &parent, int first, int last);
    void endRemoveRows();
    void dataChanged(const QModelIndex &topLeft,
                     const QModelIndex &bottomRight,
                     const QVector<int> &roles = QVector<int>());

private:
    QList<Track> list_;
};

#endif  // PLAYLISTMODEL_H
