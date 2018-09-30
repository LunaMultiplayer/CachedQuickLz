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

using System;

namespace CachedQuickLz
{
    public static partial class CachedQlz
    {
        public static byte[] Compress(byte[] source, int numBytes, out int length, int level = 3)
        {
            if (level != 1 && level != 3)
                throw new ArgumentException("C# version only supports level 1 and 3");

            length = 0;

            var src = 0;
            var dst = QlzConstants.DefaultHeaderlen + QlzConstants.CwordLen;
            var cwordVal = 0x80000000;
            var cwordPtr = QlzConstants.DefaultHeaderlen;
            var destination = ArrayPool<byte>.Spawn(numBytes + 400);
            var cachetable = ArrayPool<int>.Spawn(QlzConstants.HashValues);
            var hashCounter = ArrayPool<byte>.Spawn(QlzConstants.HashValues);
            byte[] d2;
            var fetch = 0;
            var lastMatchstart = numBytes - QlzConstants.UnconditionalMatchlen - QlzConstants.UncompressedEnd - 1;
            var lits = 0;

            var hashtable = ArrayPoolBase.SpawnHashtable(level == 1 ? 1 : 3);

            if (numBytes == 0)
                return new byte[0];

            if (src <= lastMatchstart)
                fetch = source[src] | (source[src + 1] << 8) | (source[src + 2] << 16);

            while (src <= lastMatchstart)
            {
                if ((cwordVal & 1) == 1)
                {
                    if (src > numBytes >> 1 && dst > src - (src >> 5))
                    {
                        d2 = ArrayPool<byte>.Spawn(numBytes + QlzConstants.DefaultHeaderlen);
                        WriteHeader(d2, level, false, numBytes, numBytes + QlzConstants.DefaultHeaderlen);
                        Array.Copy(source, 0, d2, QlzConstants.DefaultHeaderlen, numBytes);

                        ArrayPool<byte>.Recycle(destination);
                        ArrayPool<int>.Recycle(cachetable);
                        ArrayPool<byte>.Recycle(hashCounter);
                        ArrayPoolBase.RecycleHashtable(hashtable);

                        return d2;
                    }

                    Fastwrite(destination, cwordPtr, (int)((cwordVal >> 1) | 0x80000000), 4);
                    cwordPtr = dst;
                    dst += QlzConstants.CwordLen;
                    cwordVal = 0x80000000;
                }

                if (level == 1)
                {
                    var hash = ((fetch >> 12) ^ fetch) & (QlzConstants.HashValues - 1);
                    var o = hashtable[hash, 0];
                    var cache = cachetable[hash] ^ fetch;
                    cachetable[hash] = fetch;
                    hashtable[hash, 0] = src;

                    if (cache == 0 && hashCounter[hash] != 0 && (src - o > QlzConstants.Minoffset || src == o + 1 && lits >= 3 && src > 3 && source[src] == source[src - 3] && source[src] == source[src - 2] && source[src] == source[src - 1] && source[src] == source[src + 1] && source[src] == source[src + 2]))
                    {
                        cwordVal = (cwordVal >> 1) | 0x80000000;
                        if (source[o + 3] != source[src + 3])
                        {
                            var f = 3 - 2 | (hash << 4);
                            destination[dst + 0] = (byte)(f >> 0 * 8);
                            destination[dst + 1] = (byte)(f >> 1 * 8);
                            src += 3;
                            dst += 2;
                        }
                        else
                        {
                            var oldSrc = src;
                            var remaining = numBytes - QlzConstants.UncompressedEnd - src + 1 - 1 > 255 ? 255 : numBytes - QlzConstants.UncompressedEnd - src + 1 - 1;

                            src += 4;
                            if (source[o + src - oldSrc] == source[src])
                            {
                                src++;
                                if (source[o + src - oldSrc] == source[src])
                                {
                                    src++;
                                    while (source[o + (src - oldSrc)] == source[src] && src - oldSrc < remaining)
                                        src++;
                                }
                            }

                            var matchlen = src - oldSrc;

                            hash <<= 4;
                            if (matchlen < 18)
                            {
                                var f = hash | (matchlen - 2);
                                destination[dst + 0] = (byte)(f >> 0 * 8);
                                destination[dst + 1] = (byte)(f >> 1 * 8);
                                dst += 2;
                            }
                            else
                            {
                                Fastwrite(destination, dst, hash | (matchlen << 16), 3);
                                dst += 3;
                            }
                        }
                        fetch = source[src] | (source[src + 1] << 8) | (source[src + 2] << 16);
                        lits = 0;
                    }
                    else
                    {
                        lits++;
                        hashCounter[hash] = 1;
                        destination[dst] = source[src];
                        cwordVal = cwordVal >> 1;
                        src++;
                        dst++;
                        fetch = ((fetch >> 8) & 0xffff) | (source[src + 2] << 16);
                    }

                }
                else
                {
                    fetch = source[src] | (source[src + 1] << 8) | (source[src + 2] << 16);

                    int o;
                    int k;
                    var remaining = numBytes - QlzConstants.UncompressedEnd - src + 1 - 1 > 255 ? 255 : numBytes - QlzConstants.UncompressedEnd - src + 1 - 1;
                    var hash = ((fetch >> 12) ^ fetch) & (QlzConstants.HashValues - 1);

                    var c = hashCounter[hash];
                    var matchlen = 0;
                    var offset2 = 0;
                    for (k = 0; k < QlzConstants.QlzPointers3 && c > k; k++)
                    {
                        o = hashtable[hash, k];
                        if ((byte)fetch == source[o] && (byte)(fetch >> 8) == source[o + 1] && (byte)(fetch >> 16) == source[o + 2] && o < src - QlzConstants.Minoffset)
                        {
                            var m = 3;
                            while (source[o + m] == source[src + m] && m < remaining)
                                m++;
                            if (m > matchlen || m == matchlen && o > offset2)
                            {
                                offset2 = o;
                                matchlen = m;
                            }
                        }
                    }
                    o = offset2;
                    hashtable[hash, c & (QlzConstants.QlzPointers3 - 1)] = src;
                    c++;
                    hashCounter[hash] = c;

                    if (matchlen >= 3 && src - o < 131071)
                    {
                        var offset = src - o;

                        for (var u = 1; u < matchlen; u++)
                        {
                            fetch = source[src + u] | (source[src + u + 1] << 8) | (source[src + u + 2] << 16);
                            hash = ((fetch >> 12) ^ fetch) & (QlzConstants.HashValues - 1);
                            c = hashCounter[hash]++;
                            hashtable[hash, c & (QlzConstants.QlzPointers3 - 1)] = src + u;
                        }

                        src += matchlen;
                        cwordVal = (cwordVal >> 1) | 0x80000000;

                        if (matchlen == 3 && offset <= 63)
                        {
                            Fastwrite(destination, dst, offset << 2, 1);
                            dst++;
                        }
                        else if (matchlen == 3 && offset <= 16383)
                        {
                            Fastwrite(destination, dst, (offset << 2) | 1, 2);
                            dst += 2;
                        }
                        else if (matchlen <= 18 && offset <= 1023)
                        {
                            Fastwrite(destination, dst, ((matchlen - 3) << 2) | (offset << 6) | 2, 2);
                            dst += 2;
                        }
                        else if (matchlen <= 33)
                        {
                            Fastwrite(destination, dst, ((matchlen - 2) << 2) | (offset << 7) | 3, 3);
                            dst += 3;
                        }
                        else
                        {
                            Fastwrite(destination, dst, ((matchlen - 3) << 7) | (offset << 15) | 3, 4);
                            dst += 4;
                        }
                        lits = 0;
                    }
                    else
                    {
                        destination[dst] = source[src];
                        cwordVal = cwordVal >> 1;
                        src++;
                        dst++;
                    }
                }
            }
            while (src <= numBytes - 1)
            {
                if ((cwordVal & 1) == 1)
                {
                    Fastwrite(destination, cwordPtr, (int)((cwordVal >> 1) | 0x80000000), 4);
                    cwordPtr = dst;
                    dst += QlzConstants.CwordLen;
                    cwordVal = 0x80000000;
                }

                destination[dst] = source[src];
                src++;
                dst++;
                cwordVal = cwordVal >> 1;
            }
            while ((cwordVal & 1) != 1)
            {
                cwordVal = cwordVal >> 1;
            }
            Fastwrite(destination, cwordPtr, (int)((cwordVal >> 1) | 0x80000000), QlzConstants.CwordLen);
            WriteHeader(destination, level, true, numBytes, dst);
            d2 = ArrayPool<byte>.Spawn(dst);
            Array.Copy(destination, d2, dst);

            ArrayPool<byte>.Recycle(destination);
            ArrayPool<int>.Recycle(cachetable);
            ArrayPool<byte>.Recycle(hashCounter);
            ArrayPoolBase.RecycleHashtable(hashtable);

            length = dst;
            return d2;
        }
    }
}