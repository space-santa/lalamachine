#ifndef ENGINELOADER_H
#define ENGINELOADER_H

#include <QObject>
#include <QQmlApplicationEngine>

class EngineLoader : public QObject
{
    Q_OBJECT
public slots:
    void load() { engine_.load(QUrl(QStringLiteral("qrc:/main.qml"))); }

private:
    QQmlApplicationEngine engine_;
};

#endif  // ENGINELOADER_H
