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

#ifndef CONFIG_H
#define CONFIG_H

#include <QQuickItem>
#include <QJsonObject>

class Config : public QQuickItem
{
    Q_OBJECT

    Q_PROPERTY(double volume READ volume WRITE setVolume NOTIFY volumeChanged)

    Q_PROPERTY(QString libPath
               READ libPath
               WRITE setLibPath
               NOTIFY libPathChanged)

public:
    explicit Config(QQuickItem *parent = 0);

    static const QString LALADIR;
    static const QString CONFIGPATH;
    static const QString PLAYLISTDIR;
    static const QString MUSICLIBPATH;

    Q_INVOKABLE void saveConfig();
    void loadConfig();

    void setVolume(double val);
    double volume();

    void setLibPath(const QString &path);
    QString libPath() const;

    static QJsonObject loadJsonFile(const QString &path);
    static void saveJsonFile(const QString &path, const QJsonObject &obj);

signals:
    void volumeChanged();
    void libPathChanged();

public slots:

private:
    QJsonObject config_;

};

#endif // CONFIG_H
