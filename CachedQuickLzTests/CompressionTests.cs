using CachedQuickLz;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace CachedQuickLzTests
{
    [TestClass]
    public class CompressionTests
    {
        [TestMethod]
        public void CompressData_ImpossibleToCompress()
        {
            var data = new byte[5000];
            new Random().NextBytes(data);
            CachedQlz.Compress(data, data.Length, out var compressedLength);

            Assert.IsTrue(data.Length <= compressedLength);
        }

        [TestMethod]
        public void CompressData_NoIssues()
        {
            var text = Encoding.ASCII.GetBytes(TestCommon.RandomString(5000));
            CachedQlz.Compress(text, text.Length, out var compressedLength);

            Assert.IsTrue(text.Length > compressedLength);
        }

        [TestMethod]
        public void CompressDataReuseArrays()
        {
            //Compress a text that uses 4500 bytes
            var text = Encoding.ASCII.GetBytes(TestCommon.RandomString(4500));
            CachedQlz.Compress(text, text.Length, out _);

            //Now compress another text that uses 7500 bytes. As it has the same 
            //"next exponential value of 2", it should reuse the array
            text = Encoding.ASCII.GetBytes(TestCommon.RandomString(7500));

            var memBefore = GC.GetTotalMemory(true);
            CachedQlz.Compress(text, text.Length, out _);
            var memAfter = GC.GetTotalMemory(true);

            Assert.IsTrue(memAfter <= memBefore);
        }
    }
}
