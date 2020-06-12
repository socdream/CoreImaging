using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CoreImaging.JPEG
{
    public class DriSegment : JpegSegment
    {
        public ushort Length { get; set; }
        public ushort RestartInterval { get; set; }

        public DriSegment(BinaryReader reader)
        {
            Length = reader.ReadUInt16();
            RestartInterval = reader.ReadUInt16();
        }
    }
}
