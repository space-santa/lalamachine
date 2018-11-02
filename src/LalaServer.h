#pragma once

#include <QObject>
#include <QProcess>
#include <QString>

#include "config.h"

class LalaServer {
public:
    bool start() {
		process_.start(execPath());

        if (!process_.waitForStarted()) {
            return false;
        }

		return true;
    }

private:
    QProcess process_;

    QString execPath() {
        return Config::execPath() + "/LalaServer/LalaServer.exe";
    }
};
