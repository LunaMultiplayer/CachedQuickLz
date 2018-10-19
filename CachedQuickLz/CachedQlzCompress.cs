using System;

namespace CachedQuickLz
{
    public static partial class CachedQlz
    {
        /// <summary>
        /// Compress the given array and returns the contents into the data parameter.
        /// Caution! As the arrays are cached the size of it might be bigger than it's contents. 
        /// Use <see cref="length"/> to check the array length
        /// </summary>
        /// <param name="data">Data to compress. The compressed data will be written into this array</param>
        /// <param name="length">Length of the source array. After compression it will contain the compressed array length</param>
        /// <param name="level">Compression level. 1 = faster but less ratio. 3 = slower but higher ratio</param>
        public static void Compress(ref byte[] data, ref int length, int level = 3)
        {                        
            if (level != 1 && level != 3)
                throw new ArgumentException("C# version only supports level 1 and 3");

            if (length == 0) return;

            var src = 0;
            var dst = QlzConstants.DefaultHeaderlen + QlzConstants.CwordLen;
            var cwordVal = 0x80000000;
            var cwordPtr = QlzConstants.DefaultHeaderlen;
            var destination = ArrayPool<byte>.Spawn(length + 500);
            var cachetable = ArrayPool<int>.Spawn(QlzConstants.HashValues);
            var hashCounter = ArrayPool<byte>.Spawn(QlzConstants.HashValues);
            byte[] d2;
            var fetch = 0;
            var lastMatchstart = length - QlzConstants.UnconditionalMatchlen - QlzConstants.UncompressedEnd - 1;
            var lits = 0;

            var hashtable = HasthablePool.SpawnHashtable(level == 1 ? 1 : 3);

            if (src <= lastMatchstart)
                fetch = data[src] | (data[src + 1] << 8) | (data[src + 2] << 16);

            while (src <= lastMatchstart)
            {
                if ((cwordVal & 1) == 1)
                {
                    if (src > length >> 1 && dst > src - (src >> 5))
                    {
                        var newLength = length + QlzConstants.DefaultHeaderlen;
                        d2 = ArrayPool<byte>.Spawn(newLength + QlzConstants.QlzTrailLength);
                        WriteHeader(d2, level, false, length, newLength);
                        Array.Copy(data, 0, d2, QlzConstants.DefaultHeaderlen, length);

                        ArrayPool<byte>.Recycle(destination);
                        ArrayPool<int>.Recycle(cachetable);
                        ArrayPool<byte>.Recycle(hashCounter);
                        ArrayPool<byte>.Recycle(data);
                        HasthablePool.RecycleHashtable(hashtable);

                        WriteTrailingBytes(d2, newLength);

                        data = d2;
                        length = newLength + QlzConstants.QlzTrailLength;
                        return;
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

                    if (cache == 0 && hashCounter[hash] != 0 && (src - o > QlzConstants.Minoffset || src == o + 1 && lits >= 3 && src > 3 && data[src] == data[src - 3] && data[src] == data[src - 2] && data[src] == data[src - 1] && data[src] == data[src + 1] && data[src] == data[src + 2]))
                    {
                        cwordVal = (cwordVal >> 1) | 0x80000000;
                        if (data[o + 3] != data[src + 3])
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
                            var remaining = length - QlzConstants.UncompressedEnd - src + 1 - 1 > 255 ? 255 : length - QlzConstants.UncompressedEnd - src + 1 - 1;

                            src += 4;
                            if (data[o + src - oldSrc] == data[src])
                            {
                                src++;
                                if (data[o + src - oldSrc] == data[src])
                                {
                                    src++;
                                    while (data[o + (src - oldSrc)] == data[src] && src - oldSrc < remaining)
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
                        fetch = data[src] | (data[src + 1] << 8) | (data[src + 2] << 16);
                        lits = 0;
                    }
                    else
                    {
                        lits++;
                        hashCounter[hash] = 1;
                        destination[dst] = data[src];
                        cwordVal = cwordVal >> 1;
                        src++;
                        dst++;
                        fetch = ((fetch >> 8) & 0xffff) | (data[src + 2] << 16);
                    }

                }
                else
                {
                    fetch = data[src] | (data[src + 1] << 8) | (data[src + 2] << 16);

                    int o;
                    int k;
                    var remaining = length - QlzConstants.UncompressedEnd - src + 1 - 1 > 255 ? 255 : length - QlzConstants.UncompressedEnd - src + 1 - 1;
                    var hash = ((fetch >> 12) ^ fetch) & (QlzConstants.HashValues - 1);

                    var c = hashCounter[hash];
                    var matchlen = 0;
                    var offset2 = 0;
                    for (k = 0; k < QlzConstants.QlzPointers3 && c > k; k++)
                    {
                        o = hashtable[hash, k];
                        if ((byte)fetch == data[o] && (byte)(fetch >> 8) == data[o + 1] && (byte)(fetch >> 16) == data[o + 2] && o < src - QlzConstants.Minoffset)
                        {
                            var m = 3;
                            while (data[o + m] == data[src + m] && m < remaining)
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
                            fetch = data[src + u] | (data[src + u + 1] << 8) | (data[src + u + 2] << 16);
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
                        destination[dst] = data[src];
                        cwordVal = cwordVal >> 1;
                        src++;
                        dst++;
                    }
                }
            }
            while (src <= length - 1)
            {
                if ((cwordVal & 1) == 1)
                {
                    Fastwrite(destination, cwordPtr, (int)((cwordVal >> 1) | 0x80000000), 4);
                    cwordPtr = dst;
                    dst += QlzConstants.CwordLen;
                    cwordVal = 0x80000000;
                }

                destination[dst] = data[src];
                src++;
                dst++;
                cwordVal = cwordVal >> 1;
            }
            while ((cwordVal & 1) != 1)
            {
                cwordVal = cwordVal >> 1;
            }
            Fastwrite(destination, cwordPtr, (int)((cwordVal >> 1) | 0x80000000), QlzConstants.CwordLen);
            WriteHeader(destination, level, true, length, dst);

            ArrayPool<int>.Recycle(cachetable);
            ArrayPool<byte>.Recycle(hashCounter);
            HasthablePool.RecycleHashtable(hashtable);
            ArrayPool<byte>.Recycle(data);

            d2 = ArrayPool<byte>.Spawn(dst + QlzConstants.DefaultHeaderlen + QlzConstants.QlzTrailLength);
            Array.Copy(destination, d2, dst);
            WriteTrailingBytes(d2, dst);
            ArrayPool<byte>.Recycle(destination);

            length = dst + QlzConstants.QlzTrailLength;
            data = d2;
        }
    }
}