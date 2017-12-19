using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreImaging.Tiff
{
    public class ImageFileDirectory
    {
        public List<Tag> Tags { get; set; }

        /// <summary>
        /// Offset to next IFD, if there is a next IFD, 0 otherwise
        /// </summary>
        public uint Offset { get; set; }
        public int ByteCount
        {
            get
            {
                var total = 2 + 4;

                foreach (var item in Tags)
                    total += item.ByteCount;

                return total;
            }
        }

        public ImageFileDirectory()
        {

        }

        public ImageFileDirectory(Stream stream)
        {
            var buffer = new byte[2];

            stream.Read(buffer, 0, buffer.Length);

            var count = BitConverter.ToInt16(buffer, 0);

            Tags = new List<Tag>();

            for (int i = 0; i < count; i++)
                Tags.Add(new Tag(stream));

            buffer = new byte[4];

            stream.Read(buffer, 0, buffer.Length);

            Offset = BitConverter.ToUInt32(buffer, 0);
        }

        public void Write(Stream stream, int dataOffset)
        {
            var buffer = BitConverter.GetBytes((ushort)Tags.Count);

            stream.Write(buffer, 0, buffer.Length);
            
            foreach (var item in Tags)
            {
                item.Write(stream, dataOffset);

                dataOffset += item.DataSize;
            }

            buffer = BitConverter.GetBytes(Offset);

            stream.Write(buffer, 0, buffer.Length);

            stream.Position = dataOffset;
        }
    }
}
