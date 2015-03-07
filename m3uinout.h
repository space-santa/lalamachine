#ifndef M3UINOUT_H
#define M3UINOUT_H

#include <QQuickItem>

class M3uInOut : public QQuickItem
{
    Q_OBJECT
public:
    explicit M3uInOut(QQuickItem *parent = 0);

    Q_INVOKABLE void writePlaylist(const QString &name,
                                   const QStringList files) const;
    Q_INVOKABLE QStringList readPlaylist(const QString &name) const;
    Q_INVOKABLE QStringList getPlaylistNames() const;
    Q_INVOKABLE QString m3uPath(const QString &name) const;

signals:

public slots:

private:
};

#endif // M3UINOUT_H
