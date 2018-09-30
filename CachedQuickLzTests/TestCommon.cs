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
    }
}
