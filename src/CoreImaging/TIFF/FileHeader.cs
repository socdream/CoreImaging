using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreImaging.Tiff
{
    public class FileHeader
    {
        /// <summary>
        /// Byte order indication
        /// </summary>
        public TiffByteOrder ByteOrder { get; set; }
        /// <summary>
        /// Version number (always 42)
        /// </summary>
        public short Version { get; set; } = 42;
        /// <summary>
        /// Offset to first IFD from the file start
        /// </summary>
        public uint Offset { get; set; }

        public int ByteCount { get; } = 8;

        public FileHeader()
        {

        }

        public FileHeader(Stream stream)
        {
            var buffer = new byte[8];

            stream.Read(buffer, 0, buffer.Length);

            ByteOrder = (TiffByteOrder)BitConverter.ToInt16(buffer, 0);
            Version = BitConverter.ToInt16(buffer, 2);
            Offset = BitConverter.ToUInt32(buffer, 4);
        }

        public void Write(Stream stream)
        {
            var buffer = BitConverter.GetBytes((short)ByteOrder);

            stream.Write(buffer, 0, buffer.Length);

            buffer = BitConverter.GetBytes(Version);

            stream.Write(buffer, 0, buffer.Length);

            buffer = BitConverter.GetBytes(Offset);

            stream.Write(buffer, 0, buffer.Length);
        }

        public enum TiffByteOrder : short
        {
            /// <summary>
            /// Little endian
            /// </summary>
            II = 0x4949,
            /// <summary>
            /// Big-endian
            /// </summary>
            MM = 0x4D4D
        }
    }

}
