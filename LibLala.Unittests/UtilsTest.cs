﻿using NUnit.Framework;

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
        public void RemoveFilePrefix_FilePrefix_CeanString()
        {
            var result = Utils.Utils.RemoveFilePrefix("file:////d/e/f");
            var expected = "/d/e/f";
            Assert.AreEqual(result, expected);
        }
    }
}