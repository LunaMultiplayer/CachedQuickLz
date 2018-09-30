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
            var data = new byte[5000];
            new Random().NextBytes(data);
            var compressedData = CachedQlz.Compress(data, data.Length, out _);

            var decompressedData = CachedQlz.Decompress(compressedData, out var decompressedLength);
            Assert.AreEqual(data.Length, decompressedLength);

            var sequenceEqual = true;
            for (var i = 0; i < data.Length; i++)
            {
                sequenceEqual &= data[i] == decompressedData[i];
            }

            Assert.IsTrue(sequenceEqual);
        }

        [TestMethod]
        public void DecompressData_NoIssues()
        {
            var text = Encoding.ASCII.GetBytes(TestCommon.RandomString(5000));
            var compressedText = CachedQlz.Compress(text, text.Length, out _);

            var decompressedText = CachedQlz.Decompress(compressedText, out var decompressedLength);
            Assert.AreEqual(text.Length, decompressedLength);

            var sequenceEqual = true;
            for (var i = 0; i < text.Length; i++)
            {
                sequenceEqual &= text[i] == decompressedText[i];
            }

            Assert.IsTrue(sequenceEqual);
        }

        [TestMethod]
        public void DecompressDataReuseArrays()
        {
            var text = Encoding.ASCII.GetBytes(TestCommon.RandomString(4500));
            var compressedText = CachedQlz.Compress(text, text.Length, out _);
            CachedQlz.Decompress(compressedText, out _);

            text = Encoding.ASCII.GetBytes(TestCommon.RandomString(7500));
            compressedText = CachedQlz.Compress(text, text.Length, out _);

            var memBefore = GC.GetTotalMemory(true);
            CachedQlz.Decompress(compressedText, out _);
            var memAfter = GC.GetTotalMemory(true);

            Assert.IsTrue(memAfter <= memBefore);
        }
    }
}
