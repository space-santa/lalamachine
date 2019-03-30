#pragma once

#include <QDebug>
#include <QObject>
#include <QProcess>
#include <QString>
#include <QFileInfo>

#include "config.h"

class LalaServer {
public:
    bool start() {
        process_.start(execPath());

        if (!process_.waitForStarted()) {
            qWarning() << process_.errorString() << execPath();
            return false;
        }

        return true;
    }

private:
    QProcess process_;

    QString execPath() {
        QFileInfo tmp(QString(Config::execPath() + "LalaServer/LalaServer.exe"));
        return tmp.absoluteFilePath();
    }
};
