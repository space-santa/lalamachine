#ifndef MUSICLIBSCANNER_H
#define MUSICLIBSCANNER_H

#include <QObject>
#include <QSqlDatabase>
#include <QMutex>
#include <QSharedPointer>

class MusicLibScanner : public QObject
{
    Q_OBJECT

public:
    MusicLibScanner(QObject *parent = 0);

    void setDb(QSqlDatabase *db);

    QSharedPointer<QMutex> mutex() const;
    void setMutex(const QSharedPointer<QMutex> &mutex);

public slots:
    void scanLib(const QString &path);

signals:
    void scanStarted();
    void scanComplete();

private:
    QSharedPointer<QMutex> mutex_;
    bool suffixCheck(const QString &val) const;
    QSqlDatabase *scanDb_;

    void addTrackToDB(QString album,
                      QString artist,
                      QString comment,
                      QString genre,
                      QString length,
                      QString lengthString,
                      QString mrl,
                      QString path,
                      QString title,
                      QString track,
                      QString year);
};

#endif // MUSICLIBSCANNER_H
