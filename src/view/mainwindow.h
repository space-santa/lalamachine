#pragma once

#include <QMainWindow>
#include <QQmlApplicationEngine>

class MainWindow : public QMainWindow {
    Q_OBJECT
public:
    MainWindow(QWidget* parent = Q_NULLPTR, Qt::WindowFlags flags = Qt::WindowFlags());

private:
    QQmlApplicationEngine* engine_;
};
