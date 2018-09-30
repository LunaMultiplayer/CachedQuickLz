using CachedQuickLz;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CachedQuickLzTests
{
    [TestClass]
    public class ArrayPoolTests
    {
        [TestMethod]
        public void RequestArray()
        {
            var array = ArrayPool<byte>.Spawn(4);
            Assert.AreEqual(8, array.Length);
        }

        [TestMethod]
        public void RecycleArray()
        {
            var array = ArrayPool<byte>.Spawn(4);
            ArrayPool<byte>.Recycle(array);

            var memBefore = GC.GetTotalMemory(true);
            ArrayPool<byte>.Spawn(4);
            var memAfter = GC.GetTotalMemory(true);

            Assert.IsTrue(memAfter <= memBefore);
        }
    }
}
