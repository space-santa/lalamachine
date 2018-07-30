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

#pragma once

#include <QObject>

/*!
 * \brief The LalaTypes class provides common strings and enums.
 *
 * It is a QObject with constructor and all because it is used in QML.
 */
class LalaTypes : public QObject {
    Q_OBJECT
public:
    enum Tag { TRACK, TITLE, COMMENT, GENRE, ARTIST, ALBUM, LENGTH, LENGTHSTRING, MRL, PATH, YEAR };
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

    explicit LalaTypes(QObject* parent = 0);
    ~LalaTypes();
};
