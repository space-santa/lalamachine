using Qml.Net;
using System;

namespace dotnet
{
    class Program
    {
        static int Main(string[] args)
        {
            using (var app = new QGuiApplication(args))
            {
                using (var engine = new QQmlApplicationEngine())
                {
                    // TODO: Register your .NET types.
                    Qml.Net.Qml.RegisterType<Settings>("Lala", 1, 0);
                    Qml.Net.Qml.RegisterType<PlaylistProvider>("Lala", 1, 0);
                    Qml.Net.Qml.RegisterType<Config>("Lala", 1, 0);
                    Qml.Net.Qml.RegisterType<MusicLib>("Lala", 1, 0);
                    Qml.Net.Qml.RegisterType<SysInfo>("Lala", 1, 0);
                    Qml.Net.Qml.RegisterType<LalaTypes>("Lala", 1, 0);
                    Qml.Net.Qml.RegisterType<PlaylistModel>("Lala", 1, 0);
                    // Qml.RegisterType<NetObject>("test");
                    engine.Load("../../../../qml/main.qml");

                    return app.Exec();
                }
            }
        }
    }
}
