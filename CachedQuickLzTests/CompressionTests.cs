using CachedQuickLz;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CachedQuickLzTests
{
    [TestClass]
    public class CompressionTests
    {
        [TestMethod]
        public void CompressData_ImpossibleToCompress()
        {
            var originalLength = 100;
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

            // GC.GetAllocatedBytesForCurrentThread is a more precise way to measure allocations than GC.GetTotalMemory.
            // We expect the second compression to reuse the pooled array, resulting in zero (or very few) allocations.
            var bytesBefore = GC.GetAllocatedBytesForCurrentThread();
            CachedQlz.Compress(ref text, ref numBytes);
            var bytesAfter = GC.GetAllocatedBytesForCurrentThread();

            var allocatedBytes = bytesAfter - bytesBefore;

            // Assert that no significant memory was allocated, confirming the buffer was reused.
            // A small tolerance can be added if needed, but zero is ideal.
            Assert.IsTrue(allocatedBytes <= 128, $"Expected no more than 128 bytes allocated, but {allocatedBytes} bytes were allocated.");
        }

        [TestMethod]
        public void CompressThreadSafe()
        {
            const int iterations = 1000;
            const int originalLength = 100000;

            var task1Ok = true;
            var task1 = Task.Run(() =>
            {
                try
                {
                    for (var i = 0; i < iterations; i++)
                    {
                        var numBytes = originalLength;
                        var text = Encoding.ASCII.GetBytes(TestCommon.RandomString(numBytes));
                        CachedQlz.Compress(ref text, ref numBytes);
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
                        var numBytes = originalLength;
                        var text = Encoding.ASCII.GetBytes(TestCommon.RandomString(numBytes));
                        CachedQlz.Compress(ref text, ref numBytes);
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
