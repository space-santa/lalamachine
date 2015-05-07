#include "engineloader.h"

EngineLoader::EngineLoader(QObject *parent) : QObject(parent)
{

}

EngineLoader::~EngineLoader()
{

}

void EngineLoader::load()
{
    engine_.load(QUrl(QStringLiteral("qrc:/main.qml")));
}

