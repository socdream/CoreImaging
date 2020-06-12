using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CoreImaging.JPEG
{
    public class App0Segment : JpegSegment
    {
        public ushort Length { get; set; }
        public byte[] FileIdentifierMark { get; set; } = new byte[] { 0x4a, 0x46, 0x49, 0x46, 0x00 };
        public byte MajorRevision { get; set; }
        public byte MinorRevision { get; set; }
        public Units DensityUnits { get; set; }
        public ushort XDensity { get; set; }
        public ushort YDensity { get; set; }
        public byte ThumbnailWidth { get; set; } = 0;
        public byte ThumbnailHeigth { get; set; } = 0;

        public App0Segment(BinaryReader reader)
        {
            Length = reader.ReadUInt16();
            FileIdentifierMark = reader.ReadBytes(5);
            MajorRevision = reader.ReadByte();
            MinorRevision = reader.ReadByte();
            DensityUnits = (Units)reader.ReadByte();
            XDensity = reader.ReadUInt16();
            YDensity = reader.ReadUInt16();
            ThumbnailWidth = reader.ReadByte();
            ThumbnailHeigth = reader.ReadByte();

            // n bytes For thumbnail (RGB 24 bit)
            // we're ingnoring them for now
            reader.BaseStream.Position += ThumbnailWidth * ThumbnailHeigth * 3;
        }

        public enum Units : byte
        {
            NoUnits = 0,
            DotsInch = 1,
            DotsCm = 2
        }
    }
}
