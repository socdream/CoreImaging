using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CoreImaging.JPEG
{
    public class DhtSegment : JpegSegment
    {
        public ushort Length { get; set; }
        public byte HtNumber { get; set; }
        public HtTypes HtType { get; set; }
        public byte[] SymbolsCount { get; set; }
        public byte[] Symbols { get; set; }

        public DhtSegment(BinaryReader reader)
        {
            Length = reader.ReadUInt16();
            var ht = reader.ReadByte();

            HtNumber = (byte)(ht & 0x07);
            HtType = (HtTypes)((ht >> 4) & 0x01);
            SymbolsCount = reader.ReadBytes(16);
            Symbols = reader.ReadBytes(SymbolsCount.Select(a => (int)a).Sum());
        }

        public enum HtTypes : byte
        {
            Dc,
            Ac
        }
    }
}
