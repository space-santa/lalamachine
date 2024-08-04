using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LibLala.Utils
{
    public static class StringUtils
    {
        public static string RemoveFilePrefix(string path)
        {
            ArgumentNullException.ThrowIfNull(path);

            if (path.StartsWith("file:///", StringComparison.Ordinal))
            {
                path = path.Remove(0, 8);
            }
            return path;
        }

        public static IList<string> SafeStringList(IList<string?> unsafeList)
        {
            ArgumentNullException.ThrowIfNull(unsafeList);

            var safeList = new List<string>();
            foreach (var value in unsafeList)
            {
                if (value is { })
                {
                    safeList.Add(value);
                }
            }
            return safeList;

        }
    }
}
