#ifndef PLAYLISTSORTER_H
#define PLAYLISTSORTER_H

#include <QQuickItem>
#include <QJsonObject>
#include <QJsonArray>

class PlaylistSorter : public QQuickItem
{
    Q_OBJECT
public:
    enum SortWhat {
        TRACK,
        TITLE,
        COMMENT,
        LENGTH,
        GENRE,
        ARTIST
    };
    Q_ENUMS(SortWhat)

    enum SortHow {
        ASCENDING,
        DESCENDING
    };
    Q_ENUMS(SortHow)

    PlaylistSorter();
    ~PlaylistSorter();

    Q_INVOKABLE QJsonArray sort(QJsonArray list,
                                 SortWhat what,
                                 SortHow how) const;

signals:

public slots:

private:
    //bool titleAscending(QJsonValue i, QJsonValue j);
};

#endif // PLAYLISTSORTER_H
