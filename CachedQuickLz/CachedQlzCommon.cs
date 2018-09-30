﻿using System.Collections.Generic;

namespace CachedQuickLz
{
    public static partial class CachedQlz
    {
        /// <summary>
        /// Returns the decompressed size of the given compressed array
        /// </summary>
        /// <param name="source">Array with compressed bytes</param>
        /// <returns>Size of this array once it has been decompressed</returns>
        public static int SizeDecompressed(byte[] source)
        {
            if (HeaderLen(source) == 9)
                return source[5] | (source[6] << 8) | (source[7] << 16) | (source[8] << 24);

            return source[2];
        }

        /// <summary>
        /// Returns the compressed size of the given compressed array
        /// </summary>
        /// <param name="source">Array with compressed bytes</param>
        /// <returns>Size of this compressed array without it's header data</returns>
        public static int SizeCompressed(byte[] source)
        {
            if (HeaderLen(source) == 9)
                return source[1] | (source[2] << 8) | (source[3] << 16) | (source[4] << 24);

            return source[1];
        }

        private static int HeaderLen(IList<byte> source)
        {
            return (source[0] & 2) == 2 ? 9 : 3;
        }

        private static void WriteHeader(IList<byte> dst, int level, bool compressible, int sizeCompressed, int sizeDecompressed)
        {
            dst[0] = (byte)(2 | (compressible ? 1 : 0));
            dst[0] |= (byte)(level << 2);
            dst[0] |= 1 << 6;
            dst[0] |= 0 << 4;
            Fastwrite(dst, 1, sizeDecompressed, 4);
            Fastwrite(dst, 5, sizeCompressed, 4);
        }
        
        private static void Fastwrite(IList<byte> a, int i, int value, int numbytes)
        {
            for (var j = 0; j < numbytes; j++)
                a[i + j] = (byte)(value >> (j * 8));
        }
    }
}