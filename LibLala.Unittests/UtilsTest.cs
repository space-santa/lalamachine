using System.Collections.Generic;
using NUnit.Framework;

namespace LibLala.Unittests
{
    internal sealed class UtilsTest
    {
        [Test]
        public void SafeStringList_NullList_ReturnEmptyList()
        {
            var unsafeList = new List<string?> { null, null };
            var safeList = Utils.StringUtils.SafeStringList(unsafeList);
            Assert.That(safeList.Count == 0);
        }

        [Test]
        public void SafeStringList_ListWithStringsAndNulls_ReturnOnlyStringsInList()
        {
            var unsafeList = new List<string?> { null, "bob", null, "joe" };
            var safeList = Utils.StringUtils.SafeStringList(unsafeList);
            Assert.That(safeList.Count == 2);
            Assert.That(safeList[0] == "bob");
            Assert.That(safeList[1] == "joe");
        }
    }
}
