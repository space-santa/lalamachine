#include "theplayer.h"

#include "theplayeventhandler.h"

#include <QDebug>

ThePlayer::ThePlayer(QObject *parent)
    : QObject(parent),
      inst_(libvlc_new(0, NULL)),
      mp_(libvlc_media_player_new(inst_))
{
    connect(ThePlayEventHandler::instance(),
            &ThePlayEventHandler::playing,
            this,
            &ThePlayer::playing);
    connect(ThePlayEventHandler::instance(),
            &ThePlayEventHandler::positionChanged,
            this,
            &ThePlayer::positionChanged);
    connect(ThePlayEventHandler::instance(),
            &ThePlayEventHandler::paused,
            this,
            &ThePlayer::paused);
    connect(ThePlayEventHandler::instance(),
            &ThePlayEventHandler::stopped,
            this,
            &ThePlayer::stopped);
    connect(ThePlayEventHandler::instance(),
            &ThePlayEventHandler::end,
            this,
            &ThePlayer::atEnd);
    connect(ThePlayEventHandler::instance(),
            &ThePlayEventHandler::error,
            this,
            &ThePlayer::error);

    libvlc_event_manager_t *evt = libvlc_media_player_event_manager(mp_);
    libvlc_event_attach(evt,
                        libvlc_MediaPlayerPlaying,
                        &ThePlayEventHandler::playEventHandler,
                        nullptr);
    libvlc_event_attach(evt,
                        libvlc_MediaPlayerPositionChanged,
                        &ThePlayEventHandler::positionEventHandler,
                        nullptr);
    libvlc_event_attach(evt,
                        libvlc_MediaPlayerPaused,
                        &ThePlayEventHandler::pauseEventHandler,
                        nullptr);
    libvlc_event_attach(evt,
                        libvlc_MediaPlayerStopped,
                        &ThePlayEventHandler::stopEventHandler,
                        nullptr);
    libvlc_event_attach(evt,
                        libvlc_MediaPlayerEndReached,
                        &ThePlayEventHandler::endEventHandler,
                        nullptr);
    libvlc_event_attach(evt,
                        libvlc_MediaPlayerEncounteredError,
                        &ThePlayEventHandler::errorEventHandler,
                        nullptr);

    connect(this, &ThePlayer::atEnd, this, &ThePlayer::onEndReached);
}

ThePlayer::~ThePlayer()
{
    libvlc_media_player_release(mp_);
    libvlc_release(inst_);
}

void ThePlayer::play()
{
    libvlc_media_player_play(mp_);
    emit isPlayingChanged();
}

void ThePlayer::play(QString mrl)
{
    libvlc_media_t *m;
    m = libvlc_media_new_path(inst_, mrl.toStdString().c_str());
    libvlc_media_player_set_media(mp_, m);
    libvlc_media_release(m);
    emit trackChanged();
    play();
}

bool ThePlayer::hasAudio()
{
    bool retval = false;
    libvlc_media_t *m = libvlc_media_player_get_media(mp_);

    if (m) {
        retval = true;
    }

    libvlc_media_release(m);
    return retval;
}

void ThePlayer::seek(qint64 pos)
{
    libvlc_media_player_set_time(mp_, pos);
}

QUrl ThePlayer::source()
{
    QUrl retval;
    libvlc_media_t *m = libvlc_media_player_get_media(mp_);
    if (m) {
        retval = QUrl(QString(libvlc_media_get_mrl(m)));
    }
    libvlc_media_release(m);
    return retval;
}

void ThePlayer::pause()
{
    libvlc_media_player_pause(mp_);
    emit isPlayingChanged();
}

void ThePlayer::stop()
{
    libvlc_media_player_stop(mp_);
    emit isPlayingChanged();
}

void ThePlayer::onEndReached()
{
    qDebug() << "onStopped, loops_ =" << loops_;
    if (loops_) {
        stop();
        play();
    } else {
        emit playNext();
    }
}

bool ThePlayer::isPlaying() const
{
    return libvlc_media_player_is_playing(mp_) == 1;
}

qint64 ThePlayer::duration() const
{
    qint64 retval;
    libvlc_media_t *m = libvlc_media_player_get_media(mp_);

    if (m) {
        // If the media is not parsed, there will be no duration.
        libvlc_media_parse(m);
        retval = (qint64)libvlc_media_get_duration(m);
    }

    libvlc_media_release(m);
    return retval;
}

qint64 ThePlayer::position() const
{
    return (qint64)libvlc_media_player_get_time(mp_);
}

void ThePlayer::setVolume(int volume)
{
    libvlc_audio_set_volume(mp_, volume);
    emit volumeChanged();
}

int ThePlayer::volume() const
{
    return libvlc_audio_get_volume(mp_);
}

void ThePlayer::setMuted(bool muted)
{
    libvlc_audio_set_mute(mp_, muted);
    emit mutedChanged();
}

bool ThePlayer::muted() const
{
    qDebug() << libvlc_audio_get_mute(mp_);
    return libvlc_audio_get_mute(mp_);
}

bool ThePlayer::loops() const
{
    return loops_;
}

void ThePlayer::setLoops(bool loops)
{
    loops_ = loops;
    emit loopsChanged();
}
