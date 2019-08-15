using System.IO;

namespace LibLala
{
    public static class Utils
    {
        public static string RemoveFilePrefix(string path)
        {
            if (path.StartsWith("file:///"))
            {
                path = path.Remove(0, 8);
            }
            return path;
        }

        public static void EnsureLaladir()
        {
            Directory.CreateDirectory(path: Constants.LALADIR);
        }
    }
}
