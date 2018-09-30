using System;
using System.Collections.Concurrent;

namespace CachedQuickLz
{
    internal class HasthablePool
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
}
