using System;
using System.Linq;

namespace CachedQuickLzTests
{
    public class TestCommon
    {
        public static string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEF";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static T[] CloneArray<T>(T[] sourceArray)
        {
            var clone = new T[sourceArray.Length];
            Array.Copy(sourceArray, clone, sourceArray.Length);

            return clone;
        }

        public static T[] CloneArray<T>(T[] sourceArray, int length)
        {
            var clone = new T[sourceArray.Length];
            Array.Copy(sourceArray, clone, length);

            return clone;
        }
    }
}
