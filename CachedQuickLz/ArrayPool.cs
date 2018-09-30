using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CachedQuickLz
{
    public class ArrayPool<T>
    {
        private static readonly ConcurrentDictionary<int, Stack<T[]>> Bins;

        static ArrayPool()
        {
            Bins = new ConcurrentDictionary<int, Stack<T[]>>
            {
                [0] = new Stack<T[]>()
            };

            for (var i = 0; i < 32; i++)
            {
                Bins[1 << i] = new Stack<T[]>();
            }
        }

        internal static T[] Spawn(int minLength)
        {
            var count = NextPowerOfTwo(minLength);
            var bin = Bins[count];

            return bin.Count > 0 ? bin.Pop() : new T[count];
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
