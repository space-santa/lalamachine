#ifndef THEPLAYEVENTHANDLER_H
#define THEPLAYEVENTHANDLER_H

#include <QObject>
#include <libvlc.h>

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
    explicit ThePlayEventHandler(QObject *parent = 0);
};

#endif // THEPLAYEVENTHANDLER_H
