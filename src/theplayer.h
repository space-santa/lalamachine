#pragma once

#include <QObject>
#include <QMediaPlayer>

class ThePlayer : public QMediaPlayer
{
    Q_OBJECT

    Q_PROPERTY(bool loops READ loops WRITE setLoops NOTIFY loopsChanged)
public:
    explicit ThePlayer(QObject *parent = 0);

    Q_INVOKABLE void play();
    Q_INVOKABLE void play(QString mrl);
    Q_INVOKABLE bool isPlaying();
    Q_INVOKABLE bool hasAudio();
    Q_INVOKABLE void seek(qint64 pos);
    Q_INVOKABLE QUrl source();

    bool loops() const;
    void setLoops(bool loops);

signals:
    void stopped();
    void playing();
    void paused();
    void loopsChanged();

private slots:
    void onStateChanged(QMediaPlayer::State state);
    void onStopped();

private:
    bool loops_;
};
