#include "engineloader.h"
#include <QDebug>

EngineLoader::EngineLoader(QObject *parent) : QObject(parent) {}

void EngineLoader::load()
{
    engine_.load(QUrl(QStringLiteral("qrc:/main.qml")));
}

QObject *EngineLoader::rootWin()
{
    if (engine_.rootObjects().isEmpty()) return nullptr;
    return engine_.rootObjects().first();
}
