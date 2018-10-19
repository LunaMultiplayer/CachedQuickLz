using System;
using System.Text;
using CachedQuickLz;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CachedQuickLzTests
{
    [TestClass]
    public class CompressionCheckTests
    {
        [TestMethod]
        public void TestCheckArrayIsCompressed_ImpossibleToCompress()
        {
            var numBytes = 100;

            var data = new byte[numBytes];
            new Random().NextBytes(data);
            CachedQlz.Compress(ref data, ref numBytes);

            Assert.IsTrue(CachedQlz.IsCompressed(data, numBytes));
        }

        [TestMethod]
        public void TestCheckArrayIsCompressed_NoIssues()
        {
            var numBytes = 5000;

            var text = Encoding.ASCII.GetBytes(TestCommon.RandomString(numBytes));
            CachedQlz.Compress(ref text, ref numBytes);

            Assert.IsTrue(CachedQlz.IsCompressed(text, numBytes));
        }

        [TestMethod]
        public void TestCheckArrayIsNotCompressed()
        {
            var data = new byte[100];
            new Random().NextBytes(data);

            Assert.IsFalse(CachedQlz.IsCompressed(data, 100));
        }
    }
}
