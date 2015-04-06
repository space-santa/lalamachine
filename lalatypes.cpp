#include "lalatypes.h"

#include <QMap>

LalaTypes::LalaTypes(QObject *parent) : QObject(parent)
{

}

LalaTypes::~LalaTypes()
{

}

const QMap<LalaTypes::Tag, QString> LalaTypes::TAG_MAP = initTagMap();

QMap<LalaTypes::Tag, QString> LalaTypes::initTagMap()
{
    QMap<LalaTypes::Tag, QString> tmp;
    tmp.insert(LalaTypes::ARTIST, "artist");
    tmp.insert(LalaTypes::GENRE, "genre");
    tmp.insert(LalaTypes::TRACK, "track");
    tmp.insert(LalaTypes::TITLE, "title");
    tmp.insert(LalaTypes::COMMENT, "comment");
    tmp.insert(LalaTypes::LENGTH, "length");
    tmp.insert(LalaTypes::ALBUM, "album");
    tmp.insert(LalaTypes::LENGTHSTRING, "lengthString");
    tmp.insert(LalaTypes::MRL, "mrl");
    tmp.insert(LalaTypes::PATH, "path");
    tmp.insert(LalaTypes::YEAR, "year");

    return tmp;
}

const QMap<LalaTypes::Operator, QString> LalaTypes::OP_MAP = initOpMap();

QMap<LalaTypes::Operator, QString> LalaTypes::initOpMap()
{
    QMap<LalaTypes::Operator, QString> tmp;
    tmp.insert(LalaTypes::CONTAINS, "LIKE");
    tmp.insert(LalaTypes::CONTAINS_NOT, "NOT LIKE");
    tmp.insert(LalaTypes::IS, "IS");
    tmp.insert(LalaTypes::IS_NOT, "IS NOT");

    return tmp;
}

const QMap<LalaTypes::AndOr, QString> LalaTypes::ANDOR_MAP = initAndOrMap();

QMap<LalaTypes::AndOr, QString> LalaTypes::initAndOrMap()
{
    QMap<LalaTypes::AndOr, QString> tmp;
    tmp.insert(LalaTypes::AND, "AND");
    tmp.insert(LalaTypes::OR, "OR");

    return tmp;
}

const QString LalaTypes::AUTOPLAYLISTSUFFIX = "lala";
