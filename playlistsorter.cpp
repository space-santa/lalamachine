#include "playlistsorter.h"

#include <QDebug>
#include <QJsonObject>
#include <QJsonDocument>
#include <QJsonArray>

#include <algorithm>

#include "config.h"

bool masterSort(QVariant i,
                QVariant j,
                PlaylistSorter::SortWhat what,
                PlaylistSorter::SortHow how)
{
    Q_ASSERT(i.canConvert<QVariantMap>());
    Q_ASSERT(j.canConvert<QVariantMap>());
    QVariantMap vmap1 = i.value<QVariantMap>();
    QVariantMap vmap2 = j.value<QVariantMap>();

    QString sortString {};

    if (what == PlaylistSorter::ARTIST) {
        sortString = "artist";
    }
    if (what == PlaylistSorter::TITLE) {
        sortString = "title";
    }
    if (what == PlaylistSorter::TRACK) {
        sortString = "track";
    }
    if (what == PlaylistSorter::GENRE) {
        sortString = "genre";
    }
    if (what == PlaylistSorter::COMMENT) {
        sortString = "comment";
    }
    if (what == PlaylistSorter::LENGTH) {
        sortString = "length";
    }

    QString si = vmap1.value(sortString).toString();
    QString sj = vmap2.value(sortString).toString();

    if (how == PlaylistSorter::ASCENDING) {
        return si < sj;
    } else {
        return si > sj;
    }
}

bool artistAscending(QVariant i, QVariant j)
{
    return masterSort(i, j, PlaylistSorter::ARTIST, PlaylistSorter::ASCENDING);
}
bool artistDescending(QVariant i, QVariant j)
{
    return masterSort(i, j, PlaylistSorter::ARTIST, PlaylistSorter::DESCENDING);
}

bool titleAscending(QVariant i, QVariant j)
{
    return masterSort(i, j, PlaylistSorter::TITLE, PlaylistSorter::ASCENDING);
}
bool titleDescending(QVariant i, QVariant j)
{
    return masterSort(i, j, PlaylistSorter::TITLE, PlaylistSorter::DESCENDING);
}

bool trackAscending(QVariant i, QVariant j)
{
    return masterSort(i, j, PlaylistSorter::TRACK, PlaylistSorter::ASCENDING);
}
bool trackDescending(QVariant i, QVariant j)
{
    return masterSort(i, j, PlaylistSorter::TRACK, PlaylistSorter::DESCENDING);
}

bool genreAscending(QVariant i, QVariant j)
{
    return masterSort(i, j, PlaylistSorter::GENRE, PlaylistSorter::ASCENDING);
}
bool genreDescending(QVariant i, QVariant j)
{
    return masterSort(i, j, PlaylistSorter::GENRE, PlaylistSorter::DESCENDING);
}

bool commentAscending(QVariant i, QVariant j)
{
    return masterSort(i, j, PlaylistSorter::COMMENT, PlaylistSorter::ASCENDING);
}
bool commentDescending(QVariant i, QVariant j)
{
    return masterSort(i, j, PlaylistSorter::COMMENT, PlaylistSorter::DESCENDING);
}

bool lengthAscending(QVariant i, QVariant j)
{
    return masterSort(i, j, PlaylistSorter::LENGTH, PlaylistSorter::ASCENDING);
}
bool lengthDescending(QVariant i, QVariant j)
{
    return masterSort(i, j, PlaylistSorter::LENGTH, PlaylistSorter::DESCENDING);
}

PlaylistSorter::PlaylistSorter()
{
}

PlaylistSorter::~PlaylistSorter()
{
}

QJsonArray PlaylistSorter::sort(QJsonArray list,
                                 PlaylistSorter::SortWhat what,
                                 PlaylistSorter::SortHow how) const
{
    QVariantList varl = list.toVariantList();

    if (what == PlaylistSorter::ARTIST) {
        if (how == PlaylistSorter::ASCENDING) {
            std::sort(varl.begin(), varl.end(), artistAscending);
        } else {
            std::sort(varl.begin(), varl.end(), artistDescending);
        }
    }
    if (what == PlaylistSorter::TITLE) {
        if (how == PlaylistSorter::ASCENDING) {
            std::sort(varl.begin(), varl.end(), titleAscending);
        } else {
            std::sort(varl.begin(), varl.end(), titleDescending);
        }
    }
    if (what == PlaylistSorter::TRACK) {
        if (how == PlaylistSorter::ASCENDING) {
            std::sort(varl.begin(), varl.end(), trackAscending);
        } else {
            std::sort(varl.begin(), varl.end(), trackDescending);
        }
    }
    if (what == PlaylistSorter::GENRE) {
        if (how == PlaylistSorter::ASCENDING) {
            std::sort(varl.begin(), varl.end(), genreAscending);
        } else {
            std::sort(varl.begin(), varl.end(), genreDescending);
        }
    }
    if (what == PlaylistSorter::COMMENT) {
        if (how == PlaylistSorter::ASCENDING) {
            std::sort(varl.begin(), varl.end(), commentAscending);
        } else {
            std::sort(varl.begin(), varl.end(), commentDescending);
        }
    }
    if (what == PlaylistSorter::LENGTH) {
        if (how == PlaylistSorter::ASCENDING) {
            std::sort(varl.begin(), varl.end(), lengthAscending);
        } else {
            std::sort(varl.begin(), varl.end(), lengthDescending);
        }
    }

    return QJsonArray::fromVariantList(varl);
}

