using System;
using System.Collections.Concurrent;

namespace CachedQuickLz
{
    public class ArrayPool<T>
    {
        private static readonly ConcurrentDictionary<int, ConcurrentStack<T[]>> Bins;

        static ArrayPool()
        {
            Bins = new ConcurrentDictionary<int, ConcurrentStack<T[]>>
            {
                [0] = new ConcurrentStack<T[]>()
            };

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
            Array.Clear(array, 0, array.Length);
            var binKey = NextPowerOfTwo(array.Length + 1) / 2;

            Bins[binKey].Push(array);
        }

        private static int NextPowerOfTwo(int value)
        {
            var result = value;

            result |= result >> 1;
            result |= result >> 2;
            result |= result >> 4;
            result |= result >> 8;
            result |= result >> 16;

            return result + 1;
        }
    }
}
