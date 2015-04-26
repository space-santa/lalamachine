#ifndef FILEEXPORTER_H
#define FILEEXPORTER_H

#include <QObject>
#include <QThread>

class FileExportWorker : public QObject
{
    Q_OBJECT
public slots:
    void doExport(QString destdir, const QStringList &paths);

signals:
    void finished();
    void updateProgress(int current, int total);

private:
    QString newFileName(int pos, int max, const QString &path) const;
};

class FileExporter : public QObject
{
    Q_OBJECT
public:
    explicit FileExporter(QObject *parent = 0);
    ~FileExporter();

    Q_INVOKABLE void exportPlaylist(QString destdir, const QStringList &paths);

public slots:
    void handleUpdate(int current, int total);

signals:
    void go(QString destdir, const QStringList &paths);
    void started();
    void finished();
    void updateProgress(int val);

private:
    QThread workerThread_;
};

#endif  // FILEEXPORTER_H
