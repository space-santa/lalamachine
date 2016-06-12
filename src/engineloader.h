#ifndef ENGINELOADER_H
#define ENGINELOADER_H

#include <QObject>
#include <QQmlApplicationEngine>

class EngineLoader : public QObject
{
    Q_OBJECT
public:
    explicit EngineLoader(QObject *parent = 0);
    QObject *rootWin();

public slots:
    void load();

private:
    QQmlApplicationEngine engine_;
    QObject *rootWin_;
};

#endif  // ENGINELOADER_H
