#include "playlistmodel.h"
#include <QDebug>

PlaylistModel::PlaylistModel(QObject *parent) : QAbstractListModel(parent) {}

QHash<int, QByteArray> PlaylistModel::roleNames() const
{
    QHash<int, QByteArray> roles;
    roles[TRACK] = "track";
    roles[TITLE] = "title";
    roles[COMMENT] = "comment";
    roles[GENRE] = "genre";
    roles[ARTIST] = "artist";
    roles[ALBUM] = "album";
    roles[LENGTH] = "length";
    roles[YEAR] = "year";
    roles[DATEADDED] = "dateAdded";
    roles[ID] = "id";
    return roles;
}

// WARNING: The docs say something about table based models should return 0
// if parent is valid. Ignore that. The TableView in QML doesn't use a table
// based model. It uses a ListModel so we must return the proper count here.
int PlaylistModel::rowCount(const QModelIndex &parent) const
{
    Q_UNUSED(parent)
    return list_.length();
}

Qt::ItemFlags PlaylistModel::flags(const QModelIndex &index) const
{
    Q_UNUSED(index)
    return Qt::ItemIsSelectable | Qt::ItemIsEditable | Qt::ItemIsEnabled;
}

QVariant PlaylistModel::data(const QModelIndex &index, int role) const
{
    Track track = list_[index.row()];
    switch (role) {
        case TRACK:
            return QVariant(track.track_);
            break;
        case TITLE:
            return QVariant(track.title_);
            break;
        case COMMENT:
            return QVariant(track.comment_);
            break;
        case GENRE:
            return QVariant(track.genre_);
            break;
        case ARTIST:
            return QVariant(track.artist_);
            break;
        case ALBUM:
            return QVariant(track.album_);
            break;
        case LENGTH:
            return QVariant(track.length_);
            break;
        case YEAR:
            return QVariant(track.year_);
            break;
        case DATEADDED:
            return QVariant(track.dateAdded_);
            break;
        default:
            return QVariant();
    }
}

bool PlaylistModel::setData(const QModelIndex &index,
                            const QVariant &value,
                            int role)
{
    Q_UNUSED(index)
    Q_UNUSED(value)
    Q_UNUSED(role)
    emit dataChanged(index, index);
    return false;
}

void PlaylistModel::append(Track track)
{
    emit beginInsertRows(createIndex(list_.count(), 0),
                         list_.count(),
                         list_.count() + 1);
    list_.append(track);
    emit endInsertRows();
}

int PlaylistModel::count() const { return rowCount(); }

void PlaylistModel::move(int from, int to) { list_.move(from, to); }

void PlaylistModel::remove(int row)
{
    emit beginRemoveRows(QModelIndex(), row, row);
    list_.removeAt(row);
    emit endRemoveRows();
}

QJsonObject PlaylistModel::get(int row) { return list_.at(row).toJson(); }

void PlaylistModel::clear()
{
    emit beginRemoveRows(QModelIndex(), 0, list_.count() - 1);
    list_ = QList<Track>();
    emit endRemoveRows();
}

void PlaylistModel::append(const QJsonObject &json)
{
    emit beginInsertRows(createIndex(list_.count(), 0),
                         list_.count(),
                         list_.count() + 1);
    list_.append(Track(json));
    emit endInsertRows();
}
