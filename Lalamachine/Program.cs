using Qml.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace Lalamachine
{
    class Program
    {
        public static string GetApplicationRoot()
        {
            var exePath = Path.GetDirectoryName(System.Reflection
                              .Assembly.GetExecutingAssembly().CodeBase);
            Regex appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var appRoot = appPathMatcher.Match(exePath).Value;
            return appRoot;
        }

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

#if DEBUG
                    engine.Load(Path.Combine(GetApplicationRoot(), "assets/qml/main.qml"));
#else
                    engine.Load("C:/Program Files (x86)/SpaceSanta/lalamachine/publish/assets/qml/main.qml");
#endif

                    return app.Exec();
                }
            }
        }
    }
}
