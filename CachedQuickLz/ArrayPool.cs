using System;
using System.Collections.Concurrent;

namespace CachedQuickLz
{
    public class ArrayPool<T> where T : struct  //Value types only!
    {
        private static readonly ConcurrentDictionary<int, ConcurrentStack<T[]>> Bins;

        public static int Size
        {
            get
            {
                var result = 0;
                foreach (var bin in Bins.Values)
                {
                    foreach (var array in bin)
                    {
                        result += array.Length;
                    }
                }
                return result;
            }
        }

        static ArrayPool()
        {
            Bins = new ConcurrentDictionary<int, ConcurrentStack<T[]>>();

            for (var i = 0; i < 32; i++)
            {
                Bins[1 << i] = new ConcurrentStack<T[]>();
            }
        }

        internal static T[] Spawn(int minLength)
        {
            var count = NextPowerOfTwo(minLength);
            return Bins[count].TryPop(out var array) ? array : new T[count];
        }

        internal static void Recycle(T[] array)
        {
            if (array.Length != NextPowerOfTwo(array.Length)) throw new InvalidOperationException("Trying to recycle an array that doesn't fit a bin. Memory leak. Please use arrays made with ArrayPool<T>.Spawn(int).");

            Array.Clear(array, 0, array.Length);
            var binKey = array.Length;

            Bins[binKey].Push(array);
        }

        private static int NextPowerOfTwo(int value)
        {
            if (value <= 0) return 1;

            var result = value - 1;

            result |= result >> 1;
            result |= result >> 2;
            result |= result >> 4;
            result |= result >> 8;
            result |= result >> 16;

            return result + 1;
        }
    }
}
