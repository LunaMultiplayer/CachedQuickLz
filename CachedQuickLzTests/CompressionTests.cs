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

            var memBefore = GC.GetTotalMemory(true);
            CachedQlz.Compress(ref text, ref numBytes);
            var memAfter = GC.GetTotalMemory(true);

            Assert.IsTrue(memAfter <= memBefore);
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
