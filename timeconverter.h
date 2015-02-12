#ifndef TIMECONVERTER_H
#define TIMECONVERTER_H

#include <QString>

class TimeConverter
{
public:
    TimeConverter();

    void addSec(int sec);

    QString toString();

    static const int minInSec = 60;
    static const int hourInSec = 60 * minInSec;
    static const int dayInSec = 24 * hourInSec;

    QString numToString(int num);

    void clear();

private:
    int totalSec_;
    int sec_;
    int mins_;
    int hours_;
    int days_;
};

#endif // TIMECONVERTER_H
