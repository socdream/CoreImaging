using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreImaging.PNG
{
    public class PngHeader
    {
        public byte[] Data { get; set; }
            = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        public PngHeader()
        {

        }

        public PngHeader(System.IO.Stream stream)
        {
            var buffer = new byte[Data.Length];

            stream.Read(buffer, 0, buffer.Length);

            if (!CheckBuffer(Data, buffer))
                throw new Exception("Wrong PNG header.");
        }

        public static bool CheckBuffer(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            for (var i = 0; i < a.Length; i++)
                if (a[i] != b[i])
                    return false;

            return true;
        }
    }
}
