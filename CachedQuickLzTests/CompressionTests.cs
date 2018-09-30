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
            var originalLength = 10;
            var numBytes = originalLength;

            var data = new byte[numBytes];
            new Random().NextBytes(data);
            CachedQlz.Compress(ref data, ref numBytes);

            Assert.IsTrue(originalLength <= numBytes);
        }

        [TestMethod]
        public void CompressData_NoIssues()
        {
            var originalLength = 5000;
            var numBytes = originalLength;

            var text = Encoding.ASCII.GetBytes(TestCommon.RandomString(numBytes));
            CachedQlz.Compress(ref text, ref numBytes);

            Assert.IsTrue(originalLength > numBytes);
        }

        [TestMethod]
        public void CompressDataReuseArrays()
        {
            var numBytes = 4500;

            //Compress a text that uses 4500 bytes
            var text = Encoding.ASCII.GetBytes(TestCommon.RandomString(numBytes));
            CachedQlz.Compress(ref text, ref numBytes);

            numBytes = 5500;

            //Now compress another text that uses 5500 bytes. As it has the same 
            //"next exponential value of 2", it should reuse the array
            text = Encoding.ASCII.GetBytes(TestCommon.RandomString(numBytes));

            var memBefore = GC.GetTotalMemory(true);
            CachedQlz.Compress(ref text, ref numBytes);
            var memAfter = GC.GetTotalMemory(true);

            Assert.IsTrue(memAfter <= memBefore);
        }
    }
}
