using CachedQuickLz;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;

namespace CachedQuickLzTests
{
    [TestClass]
    public class CompressionTests
    {
        public static string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [TestMethod]
        public void CompressData()
        {
            var text = Encoding.ASCII.GetBytes(RandomString(5000));
            CachedQlz.Compress(text, text.Length, out var compressedLength);

            Assert.IsTrue(text.Length > compressedLength);
        }

        [TestMethod]
        public void CompressDataReuseArrays()
        {
            //Compress a text that uses 4500 bytes
            var text = Encoding.ASCII.GetBytes(RandomString(4500));
            CachedQlz.Compress(text, text.Length, out _);

            //Now compress another text that uses 7500 bytes. As it has the same 
            //"next exponential value of 2", it should reuse the array
            text = Encoding.ASCII.GetBytes(RandomString(7500));

            var memBefore = GC.GetTotalMemory(true);
            CachedQlz.Compress(text, text.Length, out _);
            var memAfter = GC.GetTotalMemory(true);

            Assert.IsTrue(memAfter <= memBefore);
        }
    }
}
