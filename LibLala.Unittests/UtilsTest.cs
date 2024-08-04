using System.Collections.Generic;
using NUnit.Framework;

namespace LibLala.Unittests
{
    internal sealed class UtilsTest
    {
        [Test]
        public void RemoveFilePrefix_NoPrefix_UnchangedString()
        {
            var result = Utils.StringUtils.RemoveFilePrefix("bob goes to town");
            var expected = "bob goes to town";
            Assert.That(result == expected);
        }

        [Test]
        public void RemoveFilePrefix_FilePrefix_CleanString()
        {
            var result = Utils.StringUtils.RemoveFilePrefix("file:////d/e/f");
            var expected = "/d/e/f";
            Assert.That(result == expected);
        }

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
