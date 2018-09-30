using System;

namespace CachedQuickLz
{
    public static partial class CachedQlz
    {
        /// <summary>
        /// Decompresses the given array and return the contents into the data parameter.
        /// Caution! As the arrays are cached the size of it might be bigger than it's contents.
        /// Use <see cref="length"/> to check the array length
        /// </summary>
        /// <param name="data">Data to decompress. The results will be written into this array</param>
        /// <param name="length">Length of the decompressed array</param>
        public static void Decompress(ref byte[] data, out int length)
        {
            //When decompressing an empty array, return the original empty array.  Otherwise, we'll fail trying to access source[0] later.
            if (data.Length == 0)
            {
                length = 0;
                return;
            }

            var level = (data[0] >> 2) & 0x3;
            if (level != 1 && level != 3)
            {
                throw new ArgumentException("C# version only supports level 1 and 3");
            }

            length = SizeDecompressed(data);
            var src = HeaderLen(data);
            var dst = 0;
            uint cwordVal = 1;
            var destination = ArrayPool<byte>.Spawn(length);
            var hashtable = ArrayPool<int>.Spawn(4096);
            var hashCounter = ArrayPool<byte>.Spawn(4096);
            var lastMatchstart = length - QlzConstants.UnconditionalMatchlen - QlzConstants.UncompressedEnd - 1;
            var lastHashed = -1;
            uint fetch = 0;

            if ((data[0] & 1) != 1)
            {
                Array.Copy(data, HeaderLen(data), destination, 0, length);
                ArrayPool<int>.Recycle(hashtable);
                ArrayPool<byte>.Recycle(hashCounter);
                ArrayPool<byte>.Recycle(data);

                data = destination;
                return;
            }

            for (; ; )
            {
                if (cwordVal == 1)
                {
                    cwordVal = (uint)(data[src] | (data[src + 1] << 8) | (data[src + 2] << 16) | (data[src + 3] << 24));
                    src += 4;
                    if (dst <= lastMatchstart)
                    {
                        if (level == 1)
                            fetch = (uint)(data[src] | (data[src + 1] << 8) | (data[src + 2] << 16));
                        else
                            fetch = (uint)(data[src] | (data[src + 1] << 8) | (data[src + 2] << 16) | (data[src + 3] << 24));
                    }
                }

                int hash;
                if ((cwordVal & 1) == 1)
                {
                    uint matchlen;
                    uint offset2;

                    cwordVal = cwordVal >> 1;

                    if (level == 1)
                    {
                        hash = ((int)fetch >> 4) & 0xfff;
                        offset2 = (uint)hashtable[hash];

                        if ((fetch & 0xf) != 0)
                        {
                            matchlen = (fetch & 0xf) + 2;
                            src += 2;
                        }
                        else
                        {
                            matchlen = data[src + 2];
                            src += 3;
                        }
                    }
                    else
                    {
                        uint offset;
                        if ((fetch & 3) == 0)
                        {
                            offset = (fetch & 0xff) >> 2;
                            matchlen = 3;
                            src++;
                        }
                        else if ((fetch & 2) == 0)
                        {
                            offset = (fetch & 0xffff) >> 2;
                            matchlen = 3;
                            src += 2;
                        }
                        else if ((fetch & 1) == 0)
                        {
                            offset = (fetch & 0xffff) >> 6;
                            matchlen = ((fetch >> 2) & 15) + 3;
                            src += 2;
                        }
                        else if ((fetch & 127) != 3)
                        {
                            offset = (fetch >> 7) & 0x1ffff;
                            matchlen = ((fetch >> 2) & 0x1f) + 2;
                            src += 3;
                        }
                        else
                        {
                            offset = fetch >> 15;
                            matchlen = ((fetch >> 7) & 255) + 3;
                            src += 4;
                        }
                        offset2 = (uint)(dst - offset);
                    }

                    destination[dst + 0] = destination[offset2 + 0];
                    destination[dst + 1] = destination[offset2 + 1];
                    destination[dst + 2] = destination[offset2 + 2];

                    for (var i = 3; i < matchlen; i += 1)
                    {
                        destination[dst + i] = destination[offset2 + i];
                    }

                    dst += (int)matchlen;

                    if (level == 1)
                    {
                        fetch = (uint)(destination[lastHashed + 1] | (destination[lastHashed + 2] << 8) | (destination[lastHashed + 3] << 16));
                        while (lastHashed < dst - matchlen)
                        {
                            lastHashed++;
                            hash = (int)(((fetch >> 12) ^ fetch) & (QlzConstants.HashValues - 1));
                            hashtable[hash] = lastHashed;
                            hashCounter[hash] = 1;
                            fetch = (uint)(fetch >> 8 & 0xffff | destination[lastHashed + 3] << 16);
                        }
                        fetch = (uint)(data[src] | (data[src + 1] << 8) | (data[src + 2] << 16));
                    }
                    else
                    {
                        fetch = (uint)(data[src] | (data[src + 1] << 8) | (data[src + 2] << 16) | (data[src + 3] << 24));
                    }
                    lastHashed = dst - 1;
                }
                else
                {
                    if (dst <= lastMatchstart)
                    {
                        destination[dst] = data[src];
                        dst += 1;
                        src += 1;
                        cwordVal = cwordVal >> 1;

                        if (level == 1)
                        {
                            while (lastHashed < dst - 3)
                            {
                                lastHashed++;
                                var fetch2 = destination[lastHashed] | (destination[lastHashed + 1] << 8) | (destination[lastHashed + 2] << 16);
                                hash = ((fetch2 >> 12) ^ fetch2) & (QlzConstants.HashValues - 1);
                                hashtable[hash] = lastHashed;
                                hashCounter[hash] = 1;
                            }
                            fetch = (uint)(fetch >> 8 & 0xffff | data[src + 2] << 16);
                        }
                        else
                        {
                            fetch = (uint)(fetch >> 8 & 0xffff | data[src + 2] << 16 | data[src + 3] << 24);
                        }
                    }
                    else
                    {
                        while (dst <= length - 1)
                        {
                            if (cwordVal == 1)
                            {
                                src += QlzConstants.CwordLen;
                                cwordVal = 0x80000000;
                            }

                            destination[dst] = data[src];
                            dst++;
                            src++;
                            cwordVal = cwordVal >> 1;
                        }

                        ArrayPool<int>.Recycle(hashtable);
                        ArrayPool<byte>.Recycle(hashCounter);
                        break;
                    }
                }
            }

            ArrayPool<byte>.Recycle(data);
            data = destination;
        }
    }
}