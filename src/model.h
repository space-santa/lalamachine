#pragma once

#include <QString>
#include <QStringList>


class Model
{
public:
    Model();

    QStringList genre(const QString &filter);
    QStringList artist(const QString &filter, const QString &genre = QString());
    QStringList album(const QString &filter, const QString &genre = QString(), const QString &artist = QString());
};
