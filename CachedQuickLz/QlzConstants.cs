namespace CachedQuickLz
{
    public class QlzConstants
    {
        public const int QlzTrailLength = 9;
        public static readonly byte[] QlzTrailingBytes = { 12, 8, 87, 28, 10, 88, 28, 7, 17 };

        public const int QlzVersionMajor = 1;
        public const int QlzVersionMinor = 5;
        public const int QlzVersionRevision = 0;

        // Streaming mode not supported
        public const int QlzStreamingBuffer = 0;

        // Bounds checking not supported  Use try...catch instead
        public const int QlzMemorySafe = 0;

        // Decrease QLZ_POINTERS_3 to increase level 3 compression speed. Do not edit any other values!
        public const int HashValues = 4096;
        public const int Minoffset = 2;
        public const int UnconditionalMatchlen = 6;
        public const int UncompressedEnd = 4;
        public const int CwordLen = 4;
        public const int DefaultHeaderlen = 9;
        public const int QlzPointers1 = 1;
        public const int QlzPointers3 = 16;
    }
}
