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

#include "systeminteractor.h"

#include <QDebug>

SystemInteractor::SystemInteractor(QObject *parent)
    : QObject(parent), process_(new QProcess(this))
{
}

SystemInteractor::~SystemInteractor()
{
}

QString SystemInteractor::exec(const QString &cmd)
{
    qDebug() << "SystemInteractor::exec(" << cmd << ")";
    process_->start(cmd);
    process_->waitForFinished(3000);
    parseOutput();
    process_->kill();
    return returnValue_;
}

bool SystemInteractor::startDetached(const QString &program,
                                     const QStringList &arguments) const
{
    return QProcess::startDetached(program, arguments);
}

// Reading the outputs line by line to get everything.
void SystemInteractor::parseOutput()
{
    process_->setReadChannel(QProcess::StandardError);

    while (process_->canReadLine()) {
        QString msgErr = process_->readLine();
        qDebug() << "SystemInteractor::parseOutput()" << msgErr;
    }

    process_->setReadChannel(QProcess::StandardOutput);
    QString msgOut("");

    while (process_->canReadLine()) {
        msgOut.append(QString(process_->readLine()).simplified());
    }

    qDebug() << "SystemInteractor::parseOutput()" << msgOut;
    returnValue_ = msgOut;
}
