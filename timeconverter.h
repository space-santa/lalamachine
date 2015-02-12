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

    void setSeconds(int seconds) {
        addSec(seconds);
        emit secondsChanged();
        emit timestringChanged();
    }
    int seconds() {
        return totalSec_;
    }

    QString timestring() {
        return toString();
    }

    void addSec(int sec);

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
    int totalSec_;
    int sec_;
    int mins_;
    int hours_;
    int days_;
};

#endif // TIMECONVERTER_H
