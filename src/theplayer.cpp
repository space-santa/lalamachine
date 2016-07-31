#include "theplayer.h"

ThePlayer::ThePlayer(QObject *parent) : QMediaPlayer(parent)
{
    connect(this, &QMediaPlayer::stateChanged, this, &ThePlayer::onStateChanged);
}

void ThePlayer::play()
{
    QMediaPlayer::play();
}

void ThePlayer::play(QString mrl)
{
    setMedia(QMediaContent(QUrl(mrl)));
    play();
}

bool ThePlayer::isPlaying()
{
    return state() == QMediaPlayer::PlayingState;
}

bool ThePlayer::hasAudio()
{
    return !(mediaStatus() == QMediaPlayer::NoMedia
             || mediaStatus() == QMediaPlayer::UnknownMediaStatus
             || mediaStatus() == QMediaPlayer::InvalidMedia);
}

void ThePlayer::seek(qint64 pos)
{
    qDebug() << "ThePlayer::seek(" << pos << ")" << isSeekable();
    setPosition(pos);
}

QUrl ThePlayer::source()
{
    return currentMedia().canonicalUrl();
}

void ThePlayer::onStateChanged(QMediaPlayer::State state)
{
    switch (state) {
        case QMediaPlayer::StoppedState:
            emit stopped();
            break;
    case QMediaPlayer::PlayingState:
        emit playing();
        break;
    case QMediaPlayer::PausedState:
        emit paused();
        break;
    default:
        qWarning() << "This is not supposed to happen";
    }
}

void ThePlayer::onStopped()
{
    if (loops_) {
        play();
    }
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
