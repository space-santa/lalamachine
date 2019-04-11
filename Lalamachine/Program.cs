using Qml.Net;
using System;

namespace Lalamachine
{
    class Program
    {
        static int Main(string[] args)
        {
            using (var app = new QGuiApplication(args))
            {
                using (var engine = new QQmlApplicationEngine())
                {
                    Qml.Net.Qml.RegisterType<Settings>("Lala", 1, 0);
                    Qml.Net.Qml.RegisterType<LibSettings>("Lala", 1, 0);
                    Qml.Net.Qml.RegisterType<LalaPlaylistProvider>("Lala", 1, 0);
                    Qml.Net.Qml.RegisterType<Config>("Lala", 1, 0);
                    Qml.Net.Qml.RegisterType<MusicLib>("Lala", 1, 0);
                    Qml.Net.Qml.RegisterType<SysInfo>("Lala", 1, 0);
                    Qml.Net.Qml.RegisterType<MetadataProvider>("Lala", 1, 0);
                    Qml.Net.Qml.RegisterType<Exporter>("Lala", 1, 0);
                    engine.Load("qml/main.qml");

                    return app.Exec();
                }
            }
        }
    }
}
