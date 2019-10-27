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
            const int originalLength = 128;
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

        /*
         * Because Compress will almost guaranteed create some arrays, I don't know whether this test makes sense.
         * 
        [TestMethod]
        public void CompressDecompressMemoryInvariant()
        {
            const int originalLength = 4096;
            var numBytes = originalLength;

            var text = TestCommon.RandomString(numBytes);
            var data = ArrayPool<byte>.Spawn(numBytes);

            Array.Copy(data, Encoding.ASCII.GetBytes(text), numBytes);

            CachedQlz.Compress(ref data, ref numBytes);
            CachedQlz.Decompress(ref data, out var _);

            var before = ArrayPool<byte>.Size;

            CachedQlz.Compress(ref data, ref numBytes);
            CachedQlz.Decompress(ref data, out var _);

            var after = ArrayPool<byte>.Size;

            Assert.AreEqual(before, after);
        }
        */

        [TestMethod]
        public void DecompressData_NoIssues()
        {
            const int originalLength = 4096;
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
        public void DecompressThreadSafe() //FIXME: Very vague test. Not every threading issue causes an exception. Not every test run will cause these two threads to interleave. This may be more of a static analysis task.
        {
            const int iterations = 1024;
            var length = 1024*32;

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
