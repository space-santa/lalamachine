#ifndef SYSINFO_H
#define SYSINFO_H

#include <QObject>
#include <QCoreApplication>

class SysInfo : public QObject
{
    Q_OBJECT
public:
    explicit SysInfo(QObject *parent = 0);
    ~SysInfo();

    Q_INVOKABLE QString appVersion();
    Q_INVOKABLE QString appName();
    Q_INVOKABLE QString currentCpu();
    Q_INVOKABLE QString kernelType();
    Q_INVOKABLE QString kernelVersion();
    Q_INVOKABLE QString prettyProductName();
    Q_INVOKABLE QString lalaDir();

signals:

public slots:
};

#endif // SYSINFO_H
