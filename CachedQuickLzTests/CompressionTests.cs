using CachedQuickLz;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CachedQuickLzTests
{
    [TestClass]
    public class CompressionTests
    {
        [TestMethod]
        public void CompressData_ImpossibleToCompress()
        {
            var originalLength = 128;
            var numBytes = originalLength;

            var data = new byte[numBytes];
            new Random().NextBytes(data);
            CachedQlz.Compress(ref data, ref numBytes);

            Assert.IsTrue(originalLength <= numBytes);
        }

        [TestMethod]
        public void CompressData_NoIssues()
        {
            var originalLength = 4096;
            var numBytes = originalLength;

            var text = Encoding.ASCII.GetBytes(TestCommon.RandomString(numBytes));
            CachedQlz.Compress(ref text, ref numBytes);

            Assert.IsTrue(originalLength > numBytes);
        }

        [TestMethod]
        public void CompressThreadSafe() //CAVEAT: Very vague test. Not every threading issue causes an exception. Not every test run will cause these two threads to interleave. This may be more of a static analysis task.
        {
            const int iterations = 1000;
            const int originalLength = 1024*32;

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
