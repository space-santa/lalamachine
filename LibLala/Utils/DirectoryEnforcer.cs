using System.IO;

namespace LibLala.Utils
{
    public static class DirectoryEnforcer
    {
        public static void EnsureLaladir()
        {
            Directory.CreateDirectory(path: Constants.LALADIR);
        }
    }
}
