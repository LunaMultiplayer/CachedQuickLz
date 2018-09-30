using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CachedQuickLz
{
    internal class ArrayPoolBase
    {
        protected static readonly ConcurrentBag<int[,]> Level1HashtableBag = new ConcurrentBag<int[,]>();
        protected static readonly ConcurrentBag<int[,]> Level3HashtableBag = new ConcurrentBag<int[,]>();

        internal static int[,] SpawnHashtable(int level = 3)
        {
            switch (level)
            {
                case 1:
                {
                    return Level1HashtableBag.TryTake(out var hashTable) ? hashTable :
                        new int[QlzConstants.HashValues, QlzConstants.QlzPointers1];
                }
                case 3:
                {
                    return Level3HashtableBag.TryTake(out var hashTable) ? hashTable :
                        new int[QlzConstants.HashValues, QlzConstants.QlzPointers3];
                }
                default:
                    throw new ArgumentException("C# version only supports level 1 and 3");
            }
        }

        internal static void RecycleHashtable(int[,] array)
        {
            switch (array.GetLength(1))
            {
                case QlzConstants.QlzPointers1:
                    Level1HashtableBag.Add(array);
                    break;
                case QlzConstants.QlzPointers3:
                    Level3HashtableBag.Add(array);
                    break;
                default:
                    throw new ArgumentException("Given hastable does not have the correct length");
            }
        }
    }

    internal class ArrayPool<T>: ArrayPoolBase
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
