using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LibLala.Utils
{
    public static class StringUtils
    {
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
