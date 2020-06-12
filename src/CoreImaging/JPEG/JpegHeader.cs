using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CoreImaging.JPEG
{
    public class JpegHeader
    {
        public JpegHeader(BinaryReader reader)
        {
            var data = reader.ReadByte();

            if (reader.ReadByte() != 0xff)
                throw new InvalidOperationException("This file is not a valid Jpeg");
            
            if (reader.ReadByte() != (byte)JpegImage.Marker.SOI)
                throw new InvalidOperationException("This file is not a valid Jpeg");
        }

        public static void Write(System.IO.Stream stream)
        {
            stream.Write(new byte[] { 0xff, (byte)JpegImage.Marker.SOI }, 0, 2);
        }
    }
}
