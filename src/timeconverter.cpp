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

#include "timeconverter.h"

#include <QString>

TimeConverter::TimeConverter(QObject* parent) : QObject(parent), totalSec_(0), sec_(0), mins_(0), hours_(0), days_(0) {}

void TimeConverter::setSeconds(int sec) {
    totalSec_ = sec;
    // Integer division to get the days.
    days_ = sec / dayInSec;
    // We have the days, now we only want the rest.
    sec -= days_ * dayInSec;
    // Integer division to get the hours.
    hours_ = sec / hourInSec;
    // Remove the hours.
    sec -= hours_ * hourInSec;
    // Get the rest.
    mins_ = sec / minInSec;
    sec_ = sec - mins_ * minInSec;
    // Make sure everything is good.
    Q_ASSERT(sec_ < 60);
    Q_ASSERT(sec_ >= 0);
    Q_ASSERT(mins_ < 60);
    Q_ASSERT(mins_ >= 0);
    Q_ASSERT(hours_ < 24);
    Q_ASSERT(hours_ >= 0);
    emit secondsChanged();
    emit timestringChanged();
}

int TimeConverter::seconds() {
    return totalSec_;
}

QString TimeConverter::timestring() {
    return toString();
}

QString TimeConverter::toString() {
    QString retVal("");

    if (days_ > 0) {
        retVal.append(QString::number(days_));
        if (days_ == 1) {
            retVal.append(" day, ");
        } else {
            retVal.append(" days, ");
        }
    }

    if (days_ > 0 || hours_ > 0) {
        retVal.append(numToString(hours_));
        retVal.append(":");
    }

    retVal.append(numToString(mins_));
    retVal.append(":");
    retVal.append(numToString(sec_));

    return retVal;
}

QString TimeConverter::numToString(int num) {
    QString retVal("");

    if (num < 10) {
        retVal.append("0");
    }

    retVal.append(QString::number(num));

    return retVal;
}

void TimeConverter::clear() {
    totalSec_ = 0;
    sec_ = 0;
    mins_ = 0;
    hours_ = 0;
    days_ = 0;
}
