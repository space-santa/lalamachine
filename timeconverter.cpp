#include "timeconverter.h"

#include <QString>

TimeConverter::TimeConverter() :
    totalSec_(0),
    sec_(0),
    mins_(0),
    hours_(0),
    days_(0)
{
}

void TimeConverter::addSec(int sec)
{
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
}

QString TimeConverter::toString()
{
    QString retVal("");

    if (days_ > 0) {
        retVal.append(QString::number(days_));
        retVal.append(":");
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

QString TimeConverter::numToString(int num)
{
    QString retVal("");

    if (num < 10) {
        retVal.append("0");
    }

    retVal.append(QString::number(num));

    return retVal;
}

void TimeConverter::clear()
{
    totalSec_ = 0;
    sec_ = 0;
    mins_ = 0;
    hours_ = 0;
    days_ = 0;
}
