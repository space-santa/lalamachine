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
    tmp.insert(LalaTypes::DATEADDED, "dateAdded");

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
