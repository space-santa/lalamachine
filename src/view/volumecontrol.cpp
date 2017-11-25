#include "volumecontrol.h"

#include <QHBoxLayout>
#include <QIcon>

VolumeControl::VolumeControl(QWidget *parent) : QWidget(parent) {
    createMuteButton();
    createSlider();
    createLayout();
    createConnections();
}

void VolumeControl::createMuteButton() {
    muteButton = new QPushButton(this);
    muteButton->setCheckable(true);
    setButtonIcon();
}

void VolumeControl::createSlider() {
    slider = new QSlider(this);
    slider->setMinimum(0);
    slider->setMaximum(100);
}

void VolumeControl::createLayout() {
    QHBoxLayout *hbox = new QHBoxLayout;
    hbox->addWidget(muteButton, 0);
    hbox->addWidget(slider, 1);
    setLayout(hbox);
}

void VolumeControl::createConnections() {
    connect(this, &VolumeControl::mutedChanged, this,
            &VolumeControl::setButtonIcon);
}

bool VolumeControl::getMuted() const { return muteButton->isChecked(); }

void VolumeControl::setMuted(bool value) {
    muteButton->setChecked(value);
    emit mutedChanged(value);
}

int VolumeControl::getVolume() const { return slider->value(); }

void VolumeControl::setVolume(int value) {
    if (value < 0) {
        value = 0;
    } else if (value > 100) {
        value = 100;
    }

    slider->setValue(value);
    emit volumeChanged(value);
}

void VolumeControl::setButtonIcon() {
    if (getMuted()) {
        muteButton->setIcon(QIcon("qrc:/images/images/no.png"));
    } else {
        muteButton->setIcon(QIcon("qrc:/images/images/lalamachine.png"));
    }
}

void VolumeControl::setButtonSize() {
    // Add code to scale the button with the volume.
}
