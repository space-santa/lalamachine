using System.Collections.Generic;

namespace LibLala.Utils
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

        public static List<string> SafeStringList(List<string?> unsafeList)
        {
            var safeList = new List<string>();
            foreach (var value in unsafeList)
            {
                if (value is { }) safeList.Add(value);
            }
            return safeList;

        }
    }
}
