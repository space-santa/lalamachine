using System;
using System.Collections.Generic;

namespace LibLala.Utils
{
    public static class StringUtils
    {
        public static string RemoveFilePrefix(string path)
        {
            if (path is null) throw new ArgumentNullException(paramName: nameof(path));

            if (path.StartsWith("file:///", StringComparison.Ordinal))
            {
                path = path.Remove(0, 8);
            }
            return path;
        }

        public static List<string> SafeStringList(List<string?> unsafeList)
        {
            if (unsafeList is null) throw new ArgumentNullException(paramName: nameof(unsafeList));

            var safeList = new List<string>();
            foreach (var value in unsafeList)
            {
                if (value is { }) safeList.Add(value);
            }
            return safeList;

        }
    }
}
