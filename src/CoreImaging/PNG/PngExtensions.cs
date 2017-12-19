using System;
using System.Collections.Generic;
using System.Text;

namespace CoreImaging.PNG
{
    public static class PngExtensions
    {
        static uint[] crcTable;

        // Stores a running CRC (initialized with the CRC of "IDAT" string). When
        // you write this to the PNG, write as a big-endian value
        public static uint idatCrc = Crc32(new byte[] { (byte)'I', (byte)'D', (byte)'A', (byte)'T' }, 0, 4, 0);

        // Call this function with the compressed image bytes, 
        // passing in idatCrc as the last parameter
        public static uint Crc32(this byte[] stream, int offset, int length, uint crc)
        {
            uint c;
            if (crcTable == null)
            {
                crcTable = new uint[256];
                for (uint n = 0; n <= 255; n++)
                {
                    c = n;
                    for (var k = 0; k <= 7; k++)
                    {
                        if ((c & 1) == 1)
                            c = 0xEDB88320 ^ ((c >> 1) & 0x7FFFFFFF);
                        else
                            c = ((c >> 1) & 0x7FFFFFFF);
                    }
                    crcTable[n] = c;
                }
            }
            c = crc ^ 0xffffffff;
            var endOffset = offset + length;
            for (var i = offset; i < endOffset; i++)
            {
                c = crcTable[(c ^ stream[i]) & 255] ^ ((c >> 8) & 0xFFFFFF);
            }
            return c ^ 0xffffffff;
        }
    }
}
