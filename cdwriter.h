#ifndef CDWRITER_H
#define CDWRITER_H

#include <QQuickItem>

#include "systeminteractor.h"

class CdWriter : public QQuickItem
{
    Q_OBJECT

public:
    CdWriter();
    ~CdWriter();

    bool hasK3b() const;

    Q_INVOKABLE bool burnList(const QString &m3uPath) const;

signals:

public slots:

private:
    SystemInteractor *sys_ {new SystemInteractor(this)};

    QStringList parseM3u(const QString &m3uPath) const;
};

#endif // CDWRITER_H
