#ifndef LALATYPES_H
#define LALATYPES_H

#include <QObject>

class LalaTypes : public QObject
{
    Q_OBJECT
public:
    enum Tag {
        TRACK,
        TITLE,
        COMMENT,
        GENRE,
        ARTIST,
        ALBUM,
        LENGTH,
        LENGTHSTRING,
        MRL,
        PATH,
        YEAR
    };
    Q_ENUMS(Tag)

    static const QMap<Tag, QString> TAG_MAP;
    static QMap<Tag, QString> initTagMap();

    enum Operator { CONTAINS, CONTAINS_NOT, IS, IS_NOT };
    Q_ENUMS(Operator)

    static const QMap<Operator, QString> OP_MAP;
    static QMap<Operator, QString> initOpMap();

    enum AndOr { AND, OR };
    Q_ENUMS(AndOr)

    static const QMap<AndOr, QString> ANDOR_MAP;
    static QMap<AndOr, QString> initAndOrMap();

    static const QString AUTOPLAYLISTSUFFIX;

    explicit LalaTypes(QObject *parent = 0);
    ~LalaTypes();

signals:

public slots:
};

#endif  // LALATYPES_H
