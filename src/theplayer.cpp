#include "theplayer.h"

ThePlayer::ThePlayer(QObject* parent) : QMediaPlayer(parent) {
    connect(this, &QMediaPlayer::stateChanged, this, &ThePlayer::onStateChanged);
    connect(this, &ThePlayer::stopped, this, &ThePlayer::onStopped);
}

void ThePlayer::play() {
    QMediaPlayer::play();
}

void ThePlayer::play(const QString& mrl) {
    QUrl tmp = QUrl::fromLocalFile(mrl);
    setMedia(QMediaContent(tmp));
    play();
}

bool ThePlayer::hasAudio() {
    return !(mediaStatus() == QMediaPlayer::NoMedia || mediaStatus() == QMediaPlayer::UnknownMediaStatus ||
             mediaStatus() == QMediaPlayer::InvalidMedia);
}

void ThePlayer::seek(qint64 pos) {
    setPosition(pos);
}

QUrl ThePlayer::source() {
    return currentMedia().canonicalUrl();
}

void ThePlayer::onStateChanged(QMediaPlayer::State state) {
    setIsPlaying(false);

    switch (state) {
        case QMediaPlayer::StoppedState:
            qDebug() << "--STOPPED--";
            emit stopped();
            break;
        case QMediaPlayer::PlayingState:
            qDebug() << "--PLAYING--";
            setIsPlaying(true);
            emit playing();
            break;
        case QMediaPlayer::PausedState:
            qDebug() << "--PAUSED--";
            emit paused();
            break;
        default: qWarning() << "This is not supposed to happen";
    }
}

void ThePlayer::onStopped() {
    // We only do something if it stoppes because it is at the end of the track.
    if (position() < duration()) { return; }

    qDebug() << "onStopped, loops_ =" << loops_;
    if (loops_) {
        play();
    } else {
        emit playNext();
    }
}

bool ThePlayer::isPlaying() const {
    return isPlaying_;
}

void ThePlayer::setIsPlaying(bool isPlaying) {
    isPlaying_ = isPlaying;
    emit isPlayingChanged();
}

bool ThePlayer::loops() const {
    return loops_;
}

void ThePlayer::setLoops(bool loops) {
    loops_ = loops;
    emit loopsChanged();
}
