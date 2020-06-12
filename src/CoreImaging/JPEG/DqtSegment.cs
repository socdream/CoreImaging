using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CoreImaging.JPEG
{
    public class DqtSegment : JpegSegment
    {
        public ushort Length { get; set; }
        public byte QtCount { get; set; }
        public byte Precision { get; set; }
        public byte[] Data { get; set; }

        public DqtSegment(BinaryReader reader)
        {
            Length = reader.ReadUInt16();
            var qt = reader.ReadByte();

            QtCount = (byte)(qt & 0x0F);
            Precision = (byte)((qt >> 4) & 0x0F);
            Data = reader.ReadBytes(64 * (Precision + 1));
        }
    }
}
