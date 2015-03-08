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

#ifndef TIMECONVERTER_H
#define TIMECONVERTER_H

#include <QQuickItem>
#include <QString>

class TimeConverter : public QQuickItem
{
    Q_OBJECT

    Q_PROPERTY(int seconds READ seconds WRITE setSeconds NOTIFY secondsChanged)
    Q_PROPERTY(QString timestring READ timestring NOTIFY timestringChanged)

public:
    TimeConverter(QQuickItem *parent = 0);

    void setSeconds(int sec);
    int seconds() {
        return totalSec_;
    }

    QString timestring() {
        return toString();
    }

    QString toString();

    static const int minInSec = 60;
    static const int hourInSec = 60 * minInSec;
    static const int dayInSec = 24 * hourInSec;

    QString numToString(int num);

    void clear();

signals:
    void secondsChanged();
    void timestringChanged();

private:
    int totalSec_{0};
    int sec_{0};
    int mins_{0};
    int hours_{0};
    int days_{0};
};

#endif // TIMECONVERTER_H
