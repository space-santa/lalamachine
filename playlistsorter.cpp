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

#include "playlistsorter.h"

#include <QDebug>
#include <QJsonObject>
#include <QJsonDocument>
#include <QJsonArray>

#include <algorithm>

#include "config.h"
#include "musiclib.h"

// The sort functions are not members of PlaylistSorter because their purpose
// is to be used as argument in std::sort. Nothing else. They would need to be
// static member functions to allow that.

namespace SortFunctions
{
bool masterSort(QVariant i,
                QVariant j,
                MusicLib::SortWhat what,
                MusicLib::SortHow how)
{
    Q_ASSERT(i.canConvert<QVariantMap>());
    Q_ASSERT(j.canConvert<QVariantMap>());
    QVariantMap vmap1 = i.value<QVariantMap>();
    QVariantMap vmap2 = j.value<QVariantMap>();

    QString sortString{};

    if (what == MusicLib::ALBUM) {
        sortString = "album";
    }

    if (what == MusicLib::ARTIST) {
        sortString = "artist";
    }

    if (what == MusicLib::TITLE) {
        sortString = "title";
    }

    if (what == MusicLib::TRACK) {
        sortString = "track";
        int si = vmap1.value(sortString).toInt();
        int sj = vmap2.value(sortString).toInt();

        if (how == MusicLib::ASCENDING) {
            return si < sj;
        } else {
            return si > sj;
        }
    }

    if (what == MusicLib::GENRE) {
        sortString = "genre";
    }

    if (what == MusicLib::COMMENT) {
        sortString = "comment";
    }

    if (what == MusicLib::LENGTH) {
        sortString = "length";
    }

    QString si = vmap1.value(sortString).toString();
    QString sj = vmap2.value(sortString).toString();

    if (what == MusicLib::COMMENT) {
        QRegExp rex("^\\d(A|B).*");
        if (rex.exactMatch(si)) {
            si.prepend("0");
        }

        if (rex.exactMatch(sj)) {
            sj.prepend("0");
        }
    }

    if (how == MusicLib::ASCENDING) {
        return si < sj;
    } else {
        return si > sj;
    }
}

bool albumAscending(QVariant i, QVariant j)
{
    return masterSort(i, j, MusicLib::ALBUM, MusicLib::ASCENDING);
}
bool albumDescending(QVariant i, QVariant j)
{
    return masterSort(i, j, MusicLib::ALBUM, MusicLib::DESCENDING);
}

bool artistAscending(QVariant i, QVariant j)
{
    return masterSort(i, j, MusicLib::ARTIST, MusicLib::ASCENDING);
}
bool artistDescending(QVariant i, QVariant j)
{
    return masterSort(i, j, MusicLib::ARTIST, MusicLib::DESCENDING);
}

bool titleAscending(QVariant i, QVariant j)
{
    return masterSort(i, j, MusicLib::TITLE, MusicLib::ASCENDING);
}
bool titleDescending(QVariant i, QVariant j)
{
    return masterSort(i, j, MusicLib::TITLE, MusicLib::DESCENDING);
}

bool trackAscending(QVariant i, QVariant j)
{
    return masterSort(i, j, MusicLib::TRACK, MusicLib::ASCENDING);
}
bool trackDescending(QVariant i, QVariant j)
{
    return masterSort(i, j, MusicLib::TRACK, MusicLib::DESCENDING);
}

bool genreAscending(QVariant i, QVariant j)
{
    return masterSort(i, j, MusicLib::GENRE, MusicLib::ASCENDING);
}
bool genreDescending(QVariant i, QVariant j)
{
    return masterSort(i, j, MusicLib::GENRE, MusicLib::DESCENDING);
}

bool commentAscending(QVariant i, QVariant j)
{
    return masterSort(i, j, MusicLib::COMMENT, MusicLib::ASCENDING);
}
bool commentDescending(QVariant i, QVariant j)
{
    return masterSort(i, j, MusicLib::COMMENT, MusicLib::DESCENDING);
}

bool lengthAscending(QVariant i, QVariant j)
{
    return masterSort(i, j, MusicLib::LENGTH, MusicLib::ASCENDING);
}
bool lengthDescending(QVariant i, QVariant j)
{
    return masterSort(i, j, MusicLib::LENGTH, MusicLib::DESCENDING);
}
}  // namespace SortFunctions

QJsonArray PlaylistSorter::sort(QJsonArray list, int what, int how) const
{
    QVariantList varl = list.toVariantList();

    if (what == MusicLib::ALBUM) {
        if (how == MusicLib::ASCENDING) {
            std::sort(varl.begin(), varl.end(), SortFunctions::albumAscending);
        } else {
            std::sort(varl.begin(), varl.end(), SortFunctions::albumDescending);
        }
    }

    if (what == MusicLib::ARTIST) {
        if (how == MusicLib::ASCENDING) {
            std::sort(varl.begin(), varl.end(), SortFunctions::artistAscending);
        } else {
            std::sort(varl.begin(),
                      varl.end(),
                      SortFunctions::artistDescending);
        }
    }

    if (what == MusicLib::TITLE) {
        if (how == MusicLib::ASCENDING) {
            std::sort(varl.begin(), varl.end(), SortFunctions::titleAscending);
        } else {
            std::sort(varl.begin(), varl.end(), SortFunctions::titleDescending);
        }
    }

    if (what == MusicLib::TRACK) {
        if (how == MusicLib::ASCENDING) {
            std::sort(varl.begin(), varl.end(), SortFunctions::trackAscending);
        } else {
            std::sort(varl.begin(), varl.end(), SortFunctions::trackDescending);
        }
    }

    if (what == MusicLib::GENRE) {
        if (how == MusicLib::ASCENDING) {
            std::sort(varl.begin(), varl.end(), SortFunctions::genreAscending);
        } else {
            std::sort(varl.begin(), varl.end(), SortFunctions::genreDescending);
        }
    }

    if (what == MusicLib::COMMENT) {
        if (how == MusicLib::ASCENDING) {
            std::sort(varl.begin(),
                      varl.end(),
                      SortFunctions::commentAscending);
        } else {
            std::sort(varl.begin(),
                      varl.end(),
                      SortFunctions::commentDescending);
        }
    }

    if (what == MusicLib::LENGTH) {
        if (how == MusicLib::ASCENDING) {
            std::sort(varl.begin(), varl.end(), SortFunctions::lengthAscending);
        } else {
            std::sort(varl.begin(),
                      varl.end(),
                      SortFunctions::lengthDescending);
        }
    }

    return QJsonArray::fromVariantList(varl);
}
