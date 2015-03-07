#include "systeminteractor.h"

#include <QDebug>

SystemInteractor::SystemInteractor(QObject *parent) : QObject(parent)
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
    QString msgOut {""};

    while (process_->canReadLine()) {
        msgOut.append(QString(process_->readLine()).simplified());
    }

    qDebug() << "SystemInteractor::parseOutput()" << msgOut;
    returnValue_ = msgOut;
}

