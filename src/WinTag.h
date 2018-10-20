#pragma once

#include <QJsonObject>
#include <QUrl>

#include "ITag.h"

class WinTag : public ITag {
public:
    WinTag(const QUrl& url);

    QString album() const;
    QString artist() const;
    QString comment() const;
    QString genre() const;
    QString title() const;
    QString track() const;
    QString year() const;
    QString discNumber() const;
    QString length() const;
    QString lengthString() const;
    QString path() const;

private:
    QString path_;
    QJsonObject raw_;
    void getTheTag();
};