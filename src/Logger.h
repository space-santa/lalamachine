#pragma once

#include <QDateTime>
#include <QFile>
#include <QTextStream>

#include "Config.h";

class Logger {
public:
    Logger() : theLog(Config::LOGPATH) {
        if (!theLog.open(QIODevice::Append | QIODevice::Text)) {
            return;
        }

        write("");
        write("+-----------------+");
        write("|     NEW RUN     |");
        write("+-----------------+");
    }

    void write(const QString& msg) {
        QTextStream out(&theLog);
        out << QDateTime::currentDateTime().toString("yyyy-MM-dd_hh.mm.ss.zzz") << ": " << msg << "\n";
    }

private:
    QFile theLog;
};