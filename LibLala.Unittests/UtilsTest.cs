using System.Collections.Generic;
using NUnit.Framework;

namespace LibLala.Unittests
{
    internal class UtilsTest
    {
        [Test]
        public void RemoveFilePrefix_NoPrefix_UnchangedString()
        {
            var result = Utils.Utils.RemoveFilePrefix("bob goes to town");
            var expected = "bob goes to town";
            Assert.AreEqual(result, expected);
        }

        [Test]
        public void RemoveFilePrefix_FilePrefix_CleanString()
        {
            var result = Utils.Utils.RemoveFilePrefix("file:////d/e/f");
            var expected = "/d/e/f";
            Assert.AreEqual(result, expected);
        }

        [Test]
        public void SafeStringList_NullList_ReturnEmptyList()
        {
            var unsafeList = new List<string?> { null, null };
            var safeList = Utils.Utils.SafeStringList(unsafeList);
            Assert.IsEmpty(safeList);
        }

        [Test]
        public void SafeStringList_ListWithStringsAndNulls_ReturnOnlyStringsInList()
        {
            var unsafeList = new List<string?> { null, "bob", null, "joe" };
            var safeList = Utils.Utils.SafeStringList(unsafeList);
            Assert.IsTrue(safeList.Count == 2);
            Assert.IsTrue(safeList[0] == "bob");
            Assert.IsTrue(safeList[1] == "joe");
        }
    }
}
