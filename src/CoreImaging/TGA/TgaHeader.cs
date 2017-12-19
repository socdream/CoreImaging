using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreImaging.TGA
{
    public class TgaHeader
    {
        /// <summary>
        /// This field is a one-byte unsigned integer, specifying
        /// the length of the Image Identification Field.Its range
        /// is 0 to 255.  A value of 0 means that no Image
        /// Identification Field is included.
        /// </summary>
        public byte IdLength { get; set; }
        /// <summary>
        /// This field contains a binary 1 for Data Type 1 images.
        /// </summary>
        public byte ColorMapType { get; set; }
        public DataType DataTypeCode { get; set; }
        /// <summary>
        /// Integer ( lo-hi ) index of first color map entry.
        /// </summary>
        public short ColorMapOrigin { get; set; }
        /// <summary>
        /// Integer ( lo-hi ) count of color map entries.
        /// </summary>
        public short ColorMapLength { get; set; }
        /// <summary>
        /// Number of bits in each color map entry.  16 for
        /// the Targa 16, 24 for the Targa 24, 32 for the Targa 32.
        /// </summary>
        public byte ColorMapDepth { get; set; }
        /// <summary>
        /// Integer ( lo-hi ) X coordinate of the lower left corner
        /// of the image.
        /// </summary>
        public short XOrigin { get; set; }
        /// <summary>
        /// Integer ( lo-hi ) Y coordinate of the lower left corner
        /// of the image.
        /// </summary>
        public short YOrigin { get; set; }
        /// <summary>
        /// Integer ( lo-hi ) width of the image in pixels. 
        /// </summary>
        public short Width { get; set; }
        /// <summary>
        /// Integer ( lo-hi ) height of the image in pixels.
        /// </summary>
        public short Height { get; set; }
        /// <summary>
        /// Specifies the size of each colour value. When 24 or 32 the normal conventions apply. 
        /// For 16 bits each colour component is stored as 5 bits and the remaining bit is a binary 
        /// alpha value. The colour components are converted into single byte components by simply 
        /// shifting each component up by 3 bits (multiply by 8).
        /// </summary>
        public byte BitsPerPixel { get; set; }
        /// <summary>
        /// Bits 3-0 - number of attribute bits associated with each pixel.
        /// Bit 4    - reserved.  Must be set to 0.  
        /// Bit 5    - screen origin bit.
        ///     0 = Origin in lower left-hand corner.
        ///     1 = Origin in upper left-hand corner.
        ///     Must be 0 for Truevision images.
        /// Bits 7-6 - Data storage interleaving flag.
        ///     00 = non-interleaved.
        ///     01 = two-way (even/odd) interleaving.
        ///     10 = four way interleaving.
        ///     11 = reserved.
        /// </summary>
        public byte ImageDescriptor { get; set; }

        public TgaHeader(Stream stream)
        {
            var buffer = new byte[18];

            stream.Read(buffer, 0, buffer.Length);

            IdLength = buffer[0];
            ColorMapType = buffer[1];
            DataTypeCode = (DataType)buffer[2];
            ColorMapOrigin = BitConverter.ToInt16(buffer, 3);
            ColorMapLength = BitConverter.ToInt16(buffer, 5);
            ColorMapDepth = buffer[6];
            XOrigin = BitConverter.ToInt16(buffer, 8);
            YOrigin = BitConverter.ToInt16(buffer, 10);
            Width = BitConverter.ToInt16(buffer, 12);
            Height = BitConverter.ToInt16(buffer, 14);
            BitsPerPixel = buffer[16];
            ImageDescriptor = buffer[17];
        }

        public enum DataType : byte
        {
            NoImage = 0,
            UncompressedColorMapped = 1,
            UncompressedRGB = 2,
            UncompressedBW =3,
            RunlengthColorMapped = 9,
            RunlengthRGB = 10,
            CompressedBW = 11,
            CompressedColorMapped = 32,
            CompressedColorMapped4Pass = 33
        }
    }
}
