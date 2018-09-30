// QuickLZ data compression library
// Copyright (C) 2006-2011 Lasse Mikkel Reinhold
// lar@quicklz.com
//
// QuickLZ can be used for free under the GPL 1, 2 or 3 license (where anything 
// released into public must be open source) or under a commercial license if such 
// has been acquired (see http://www.quicklz.com/order.html). The commercial license 
// does not cover derived or ported versions created by third parties under GPL.
//
// Only a subset of the C library has been ported, namely level 1 and 3 not in 
// streaming mode. 
//
// Version: 1.5.0 final


using System.Collections.Generic;

namespace CachedQuickLz
{
    public static partial class CachedQlz
    {
        private static int HeaderLen(IList<byte> source)
        {
            return (source[0] & 2) == 2 ? 9 : 3;
        }

        public static int SizeDecompressed(byte[] source)
        {
            if (HeaderLen(source) == 9)
                return source[5] | (source[6] << 8) | (source[7] << 16) | (source[8] << 24);

            return source[2];
        }

        public static int SizeCompressed(byte[] source)
        {
            if (HeaderLen(source) == 9)
                return source[1] | (source[2] << 8) | (source[3] << 16) | (source[4] << 24);

            return source[1];
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