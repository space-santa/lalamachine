#include "sysinfo.h"

#include <QSysInfo>
#include <QDebug>

#include <config.h>

SysInfo::SysInfo(QObject *parent) : QObject(parent)
{
}

SysInfo::~SysInfo()
{
}

QString SysInfo::appVersion()
{
    return QCoreApplication::applicationVersion();
}

QString SysInfo::appName()
{
    return QCoreApplication::applicationName();
}

QString SysInfo::currentCpu()
{
    return QSysInfo::currentCpuArchitecture();
}

QString SysInfo::kernelType()
{
    return QSysInfo::kernelType();
}

QString SysInfo::kernelVersion()
{
    return QSysInfo::kernelVersion();
}

QString SysInfo::prettyProductName()
{
    return QSysInfo::prettyProductName();
}

QString SysInfo::lalaDir()
{
    return Config::LALADIR;
}
