#pragma once

#include <QObject>
#include <QUrl>

#include <vlc.h>

class ThePlayer : public QObject
{
    Q_OBJECT

    Q_PROPERTY(bool loops READ loops WRITE setLoops NOTIFY loopsChanged)
    Q_PROPERTY(bool isPlaying READ isPlaying NOTIFY isPlayingChanged)
    Q_PROPERTY(qint64 duration READ duration NOTIFY trackChanged)
    Q_PROPERTY(int volume READ volume WRITE setVolume NOTIFY volumeChanged)
    Q_PROPERTY(qint64 position READ position NOTIFY positionChanged)
    Q_PROPERTY(bool muted READ muted WRITE setMuted NOTIFY mutedChanged)

public:
    explicit ThePlayer(QObject *parent = 0);
    ~ThePlayer();

    Q_INVOKABLE void play();
    Q_INVOKABLE void play(QString mrl);
    Q_INVOKABLE bool hasAudio();
    Q_INVOKABLE void seek(qint64 pos);
    Q_INVOKABLE QUrl source();
    Q_INVOKABLE void pause();
    Q_INVOKABLE void stop();

    bool loops() const;
    void setLoops(bool loops);

    bool isPlaying() const;

    qint64 duration() const;
    qint64 position() const;

    void setVolume(int volume);
    int volume() const;

    void setMuted(bool muted);
    bool muted() const;

signals:
    void stopped();
    void playing();
    void paused();
    void loopsChanged();
    void isPlayingChanged();
    void playNext();
    void trackChanged();
    void volumeChanged();
    void positionChanged();
    void atEnd();
    void error();
    void mutedChanged();

private slots:
    void onEndReached();

private:
    bool loops_{false};
    bool muted_{false};

    libvlc_instance_t *inst_;
    libvlc_media_player_t *mp_;
};
