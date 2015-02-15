#ifndef CONFIG_H
#define CONFIG_H

#include <QQuickItem>
#include <QJsonObject>

class Config : public QQuickItem
{
    Q_OBJECT

    Q_PROPERTY(double volume READ volume WRITE setVolume NOTIFY volumeChanged)

public:
    explicit Config(QQuickItem *parent = 0);

    static const QString LALADIR;
    static const QString CONFIGPATH;
    static const QString PLAYLISTDIR;
    static const QString MUSICLIBPATH;

    Q_INVOKABLE void saveConfig();
    void loadConfig();

    void setVolume(double val);
    double volume();

    static QJsonObject loadJsonFile(const QString &path);
    static void saveJsonFile(const QString &path, const QJsonObject &obj);

signals:
    void volumeChanged();

public slots:

private:
    QJsonObject config_;

};

#endif // CONFIG_H
