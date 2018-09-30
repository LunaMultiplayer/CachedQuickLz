using CachedQuickLz;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace CachedQuickLzTests
{
    [TestClass]
    public class DecompressionTests
    {
        [TestMethod]
        public void DecompressData_ImpossibleToCompress()
        {
            var originalLength = 10;
            var numBytes = originalLength;

            var data = new byte[numBytes];
            var dataBackup = new byte[numBytes];
            new Random().NextBytes(data);
            Array.Copy(data, dataBackup, numBytes);
            
            CachedQlz.Compress(ref data, ref numBytes);
            CachedQlz.Decompress(ref data, out var decompressedLength);

            Assert.AreEqual(originalLength, decompressedLength);

            var sequenceEqual = true;
            for (var i = 0; i < originalLength; i++)
            {
                sequenceEqual &= data[i] == dataBackup[i];
            }
            Assert.IsTrue(sequenceEqual);
        }

        [TestMethod]
        public void DecompressData_NoIssues()
        {
            var originalLength = 5000;
            var numBytes = originalLength;

            var text = TestCommon.RandomString(numBytes);
            var data = Encoding.ASCII.GetBytes(text);
            var dataBackup = new byte[numBytes];
            Array.Copy(data, dataBackup, numBytes);

            CachedQlz.Compress(ref data, ref numBytes);
            CachedQlz.Decompress(ref data, out var decompressedLength);

            Assert.AreEqual(originalLength, decompressedLength);

            var sequenceEqual = true;
            for (var i = 0; i < originalLength; i++)
            {
                sequenceEqual &= data[i] == dataBackup[i];
            }
            Assert.IsTrue(sequenceEqual);
        }

        [TestMethod]
        public void DecompressDataReuseArrays()
        {
            var numBytes = 4500;

            var text = Encoding.ASCII.GetBytes(TestCommon.RandomString(numBytes));
            CachedQlz.Compress(ref text, ref numBytes);
            CachedQlz.Decompress(ref text, out _);

            numBytes = 7500;

            text = Encoding.ASCII.GetBytes(TestCommon.RandomString(numBytes));
            CachedQlz.Compress(ref text, ref numBytes);

            var memBefore = GC.GetTotalMemory(true);
            CachedQlz.Decompress(ref text, out _);
            var memAfter = GC.GetTotalMemory(true);

            Assert.IsTrue(memAfter <= memBefore);
        }
    }
}
