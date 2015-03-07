#ifndef SYSTEMINTERACTOR_H
#define SYSTEMINTERACTOR_H

#include <QObject>
#include <QProcess>

class SystemInteractor : public QObject
{
    Q_OBJECT
public:
    explicit SystemInteractor(QObject *parent = 0);
    ~SystemInteractor();

    QString exec(const QString &cmd);
    bool startDetached(const QString &program,
                       const QStringList &arguments) const;

signals:

public slots:

private slots:
    void parseOutput();

private:
    QProcess *process_ {new QProcess(this)};
    QString returnValue_ {""};
};

#endif // SYSTEMINTERACTOR_H
