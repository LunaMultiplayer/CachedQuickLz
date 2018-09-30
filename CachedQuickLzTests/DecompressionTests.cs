using CachedQuickLz;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;

namespace CachedQuickLzTests
{
    [TestClass]
    public class DecompressionTests
    {
        public static string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [TestMethod]
        public void DecompressData()
        {
            var text = Encoding.ASCII.GetBytes(RandomString(5000));
            var compressedText = CachedQlz.Compress(text, text.Length, out var compressedLength);

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
            var text = Encoding.ASCII.GetBytes(RandomString(4500));
            var compressedText = CachedQlz.Compress(text, text.Length, out _);
            CachedQlz.Decompress(compressedText, out _);

            text = Encoding.ASCII.GetBytes(RandomString(7500));
            compressedText = CachedQlz.Compress(text, text.Length, out _);

            var memBefore = GC.GetTotalMemory(true);
            CachedQlz.Decompress(compressedText, out _);
            var memAfter = GC.GetTotalMemory(true);

            Assert.IsTrue(memAfter <= memBefore);
        }
    }
}
