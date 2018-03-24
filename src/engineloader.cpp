#include "engineloader.h"
#include <QDebug>

EngineLoader::EngineLoader(QObject* parent) : QObject(parent), rootWin_(Q_NULLPTR) {}

void EngineLoader::load() {
    engine_.load(QUrl(QStringLiteral("qrc:/main.qml")));
}

// The object is owned by the engine. DO NOT DELETE.
QObject* EngineLoader::rootWin() {
    if (engine_.rootObjects().isEmpty()) { return Q_NULLPTR; }

    return engine_.rootObjects().first();
}
