using CachedQuickLz;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CachedQuickLzTests
{
    [TestClass]
    public class DecompressionTests
    {
        [TestMethod]
        public void DecompressData_ImpossibleToCompress()
        {
            const int originalLength = 100;
            var numBytes = originalLength;

            var data = new byte[numBytes];
            new Random().NextBytes(data);

            var dataBackup = TestCommon.CloneArray(data);

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
            const int originalLength = 5000;
            var numBytes = originalLength;

            var text = TestCommon.RandomString(numBytes);
            var data = Encoding.ASCII.GetBytes(text);

            var dataBackup = TestCommon.CloneArray(data);

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

            numBytes = 5500;

            text = Encoding.ASCII.GetBytes(TestCommon.RandomString(numBytes));
            CachedQlz.Compress(ref text, ref numBytes);

            var memBefore = GC.GetTotalMemory(true);
            CachedQlz.Decompress(ref text, out _);
            var memAfter = GC.GetTotalMemory(true);

            Assert.IsTrue(memAfter <= memBefore);
        }

        [TestMethod]
        public void DecompressThreadSafe()
        {
            const int iterations = 1000;
            var length = 100000;

            var data = Encoding.ASCII.GetBytes(TestCommon.RandomString(length));
            CachedQlz.Compress(ref data, ref length);

            var task1Ok = true;
            var task1 = Task.Run(() =>
            {
                try
                {
                    for (var i = 0; i < iterations; i++)
                    {
                        var clone = TestCommon.CloneArray(data);
                        CachedQlz.Decompress(ref clone, out _);
                    }
                }
                catch (Exception)
                {
                    task1Ok = false;
                }
            });

            var task2Ok = true;
            var task2 = Task.Run(() =>
            {
                try
                {
                    for (var i = 0; i < iterations; i++)
                    {
                        var clone = TestCommon.CloneArray(data);
                        CachedQlz.Decompress(ref clone, out _);
                    }
                }
                catch (Exception)
                {
                    task2Ok = false;
                }
            });

            task1.Wait();
            task2.Wait();

            Assert.IsTrue(task1Ok);
            Assert.IsTrue(task2Ok);
        }
    }
}
