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
            Assert.AreEqual(4, array.Length);
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

        [TestMethod]
        public void RecycleArrayActuallyRecycles()
        {
            var before = ArrayPool<int>.Spawn(4);
            ArrayPool<int>.Recycle(before);

            var after = ArrayPool<int>.Spawn(4);
            ArrayPool<int>.Recycle(after);

            Assert.IsTrue(before == after); //reference compare is technically enough

            //But since it's 3am and we know Javascript, we are superstitious
            var after2 = ArrayPool<int>.Spawn(4);
            before[1] = 123;
            Assert.AreEqual(after[1], 123); 
            Assert.AreEqual(after2[1], 123); //CAVEAT: This also shows there is a dangerous side effect of recycling an array that was given as a parameter. The old owner might keep using it!
        }

        [TestMethod]
        public void RecycleArrayRejectsMissizedArray()
        {
            Assert.ThrowsException<InvalidOperationException>(() => ArrayPool<byte>.Recycle(new byte[10]));
        }

        [TestMethod]
        public void SizingIsAccurate()
        {
            var test = ArrayPool<byte>.Spawn(0);
            Assert.IsTrue(test.Length == 1);            

            test = ArrayPool<byte>.Spawn(1);
            Assert.IsTrue(test.Length == 1);            

            test = ArrayPool<byte>.Spawn(2);
            Assert.IsTrue(test.Length == 2);            

            test = ArrayPool<byte>.Spawn(3);
            Assert.IsTrue(test.Length == 4);            

            test = ArrayPool<byte>.Spawn(4);
            Assert.IsTrue(test.Length == 4);            

            test = ArrayPool<byte>.Spawn(5);
            Assert.IsTrue(test.Length == 8);
            
            test = ArrayPool<byte>.Spawn(65535);
            Assert.IsTrue(test.Length == 65536);                        
        }       
    }
}
