#pragma once

#include <QPushButton>
#include <QSlider>
#include <QWidget>

class VolumeControl : public QWidget {
    Q_OBJECT
public:
    VolumeControl(QWidget* parent);

    Q_PROPERTY(bool muted READ getMuted WRITE setMuted NOTIFY mutedChanged);
    Q_PROPERTY(int volume READ getVolume WRITE setVolume NOTIFY volumeChanged);

    bool getMuted() const;
    void setMuted(bool muted);

    int getVolume() const;
    void setVolume(int volume);

signals:
    void mutedChanged(bool value);
    void volumeChanged(int value);

private slots:
    void setButtonIcon();
    void setButtonSize();

private:
    QSlider* slider;
    QPushButton* muteButton;

    void createMuteButton();
    void createSlider();
    void createLayout();
    void createConnections();
};