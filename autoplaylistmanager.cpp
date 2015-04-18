#include "autoplaylistmanager.h"

#include <QFileInfoList>
#include <QDir>

AutoPlaylistManager::AutoPlaylistManager(QObject *parent) : QObject(parent)
{
}

AutoPlaylistManager::~AutoPlaylistManager()
{
}

QStringList AutoPlaylistManager::getAutoPlaylistNames()
{
    QDir d(Config::AUTOPLAYLISTDIR);
    QStringList filters;
    filters << QString("*.") + LalaTypes::AUTOPLAYLISTSUFFIX;
    d.setNameFilters(filters);
    QStringList names;
    QFileInfoList entries = d.entryInfoList();

    for (int i = 0; i < entries.length(); ++i) {
        names.append(entries[i].baseName());
    }

    return names;
}

QJsonArray AutoPlaylistManager::getAutoPlaylist(const QString name) const
{
    QJsonArray retval;
    retval = Config::loadJsonFile(getPath(name)).value(name).toArray();
    return retval;
}

void AutoPlaylistManager::saveAutoPlaylist(const QString &name,
                                           const QJsonArray &args)
{
    QList<AutoPlaylistObject> list;

    for (int i = 0; i < args.count(); ++i) {
        list.append(AutoPlaylistObject(args[i].toObject()));
    }

    saveAutoPlaylist(name, list);
    emit autoPlaylistNamesChanged();
}

void AutoPlaylistManager::deleteAutoPlaylist(const QString &name)
{
    Q_ASSERT(QStringList(getAutoPlaylistNames()).contains(name));
    QFile::remove(getPath(name));
    emit autoPlaylistNamesChanged();
}

void AutoPlaylistManager::saveAutoPlaylist(
    const QString &name, const QList<AutoPlaylistObject> &args) const
{
    QJsonArray jarr;

    for (auto itr = args.begin(); itr != args.end(); ++itr) {
        jarr.append((*itr).toJson());
    }

    QJsonObject obj;
    obj.insert(name, jarr);

    Config::saveJsonFile(getPath(name), obj);
}

QString AutoPlaylistManager::getPath(const QString &name) const
{
    return Config::AUTOPLAYLISTDIR + "/" + name + "."
           + LalaTypes::AUTOPLAYLISTSUFFIX;
}
