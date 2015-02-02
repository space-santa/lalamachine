#ifndef M3UINOUT_H
#define M3UINOUT_H

#include <QQuickItem>

class M3uInOut : public QQuickItem
{
    Q_OBJECT
public:
    explicit M3uInOut(QQuickItem *parent = 0);

    static const QString PLAYLISTDIR;

    Q_INVOKABLE void writePlaylist(const QString &name,
                                   const QStringList files) const;
    Q_INVOKABLE QStringList readPlaylist(const QString &name) const;
    Q_INVOKABLE QStringList getPlaylistNames() const;

signals:

public slots:

private:
    QString m3uPath(const QString &name) const;
};

#endif // M3UINOUT_H
