#pragma once

#include <QMediaPlayer>
#include <QObject>
#include <QUrl>

class ThePlayer : public QMediaPlayer {
  Q_OBJECT

  Q_PROPERTY(bool loops READ loops WRITE setLoops NOTIFY loopsChanged)
  Q_PROPERTY(
      bool isPlaying READ isPlaying WRITE setIsPlaying NOTIFY isPlayingChanged)
 public:
  explicit ThePlayer(QObject* parent = 0);

  Q_INVOKABLE void play();
  Q_INVOKABLE void play(const QString& mrl);
  Q_INVOKABLE bool hasAudio();
  Q_INVOKABLE void seek(qint64 pos);
  Q_INVOKABLE QUrl source();

  bool loops() const;
  void setLoops(bool loops);
  bool isPlaying() const;

  void setIsPlaying(bool isPlaying);

 signals:
  void stopped();
  void playing();
  void paused();
  void loopsChanged();
  void isPlayingChanged();
  void playNext();

 private slots:
  void onStateChanged(QMediaPlayer::State state);
  void onStopped();

 private:
  bool loops_{false};
  bool isPlaying_{false};
};
