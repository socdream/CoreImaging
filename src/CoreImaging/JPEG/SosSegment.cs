using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CoreImaging.JPEG
{
    public class SosSegment : JpegSegment
    {
        public ushort Length { get; set; }
        public List<ComponentIds> Components { get; set; } = new List<ComponentIds>();
        public List<int> AcTables { get; set; } = new List<int>();
        public List<int> DcTables { get; set; } = new List<int>();

        public SosSegment(BinaryReader reader)
        {
            Length = reader.ReadUInt16();
            var componentsCount = reader.ReadByte();

            for (int i = 0; i < componentsCount; i++)
            {
                Components.Add((ComponentIds)reader.ReadByte());

                var value = reader.ReadByte();
                AcTables.Add(value & 0xF);
                DcTables.Add((value >> 4) & 0xF);
            }

            reader.ReadBytes(3);

            // TODO: Read image data
        }

        public enum ComponentIds : byte
        {
            Y = 1,
            Cb = 2,
            Cr = 3,
            I = 4,
            Q = 5
        }
    }
}
