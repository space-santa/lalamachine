#include "cdwriter.h"
#include "systeminteractor.h"

#include <QDebug>
#include <QString>
#include <QStringList>
#include <QFile>
#include <QTextStream>

CdWriter::CdWriter()
{

}

CdWriter::~CdWriter()
{

}

bool CdWriter::hasK3b() const
{
    QString result = sys_->exec("which k3b");

    return !result.contains("no k3b");
}

bool CdWriter::burnList(const QString &m3uPath) const
{
    if (!hasK3b()) {
        qCritical() << "K3b could not be found.";
        return false;
    }

    QString program("k3b");
    QStringList arguments;
    arguments << "--audiocd";
    arguments << parseM3u(m3uPath);

    return sys_->startDetached(program, arguments);
}

QStringList CdWriter::parseM3u(const QString &m3uPath) const
{
    QStringList retVal;
    QFile inputFile(m3uPath);
    if (inputFile.open(QIODevice::ReadOnly))
    {
       QTextStream in(&inputFile);
       while (!in.atEnd())
       {
          retVal << in.readLine();
       }
       inputFile.close();
    }

    return retVal;
}

