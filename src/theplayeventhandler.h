#pragma once

#include <QObject>
#include <libvlc.h>

/*!
 * \brief The ThePlayEventHandler class provides callback funtions for libvlc.
 *
 * The eventmanager of libvlc needs static callback functions. To make them emit
 * signals created this singleton.
 */
class ThePlayEventHandler : public QObject
{
    Q_OBJECT
public:
    static ThePlayEventHandler *instance();

    static void playEventHandler(const libvlc_event_t *, void *);
    static void pauseEventHandler(const libvlc_event_t *, void *);
    static void stopEventHandler(const libvlc_event_t *, void *);
    static void endEventHandler(const libvlc_event_t *, void *);
    static void positionEventHandler(const libvlc_event_t *, void *);
    static void errorEventHandler(const libvlc_event_t *, void *);

signals:
    void playing();
    void paused();
    void stopped();
    void end();
    void positionChanged();
    void error();

private:
    explicit ThePlayEventHandler(QObject *parent = 0) : QObject(parent)
    {
    }
};
