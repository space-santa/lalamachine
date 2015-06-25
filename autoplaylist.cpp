#include "autoplaylist.h"

AutoPlaylist::AutoPlaylist(const QString &name, QObject *parent)
    : QObject(parent), name_(name)
{
}

AutoPlaylist::~AutoPlaylist() {}

void AutoPlaylist::addApo(const AutoPlaylistObject &apo)
{
    apos_.append(apo);
}

void AutoPlaylist::clear()
{
    apos_.clear();
}

QJsonObject AutoPlaylist::list() { return QJsonObject(); }

void AutoPlaylist::setList(const QJsonObject &list)
{
    Q_UNUSED(list)
    emit listChanged();
}

QString AutoPlaylist::toQuery()
{
    QString query("SELECT * FROM musiclib WHERE ");
    bool first = true;
    for (auto itr = apos_.constBegin(); itr != apos_.constEnd(); ++itr) {
        query.append((*itr).toQuery(first));
        first = false;
    }
    query.append(" ORDER BY artist, album, track");

    return query.simplified();
}
