#include "theplayeventhandler.h"

ThePlayEventHandler *ThePlayEventHandler::instance()
{
    static ThePlayEventHandler *instance = new ThePlayEventHandler;
    return instance;
}

void ThePlayEventHandler::playEventHandler(const libvlc_event_t *, void *)
{
    emit instance()->playing();
}

void ThePlayEventHandler::pauseEventHandler(const libvlc_event_t *, void *)
{
    emit instance()->paused();
}

void ThePlayEventHandler::stopEventHandler(const libvlc_event_t *, void *)
{
    emit instance()->stopped();
}

void ThePlayEventHandler::endEventHandler(const libvlc_event_t *, void *)
{
    emit instance()->end();
}

void ThePlayEventHandler::positionEventHandler(const libvlc_event_t *, void *)
{
    emit instance()->positionChanged();
}

void ThePlayEventHandler::errorEventHandler(const libvlc_event_t *, void *)
{
    emit instance()->error();
}


