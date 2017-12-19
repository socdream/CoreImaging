using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreImaging.PNG
{
    public class PngChunk
    {
        public int Length { get; set; }
        /// <summary>
        /// 4 byte ASCII string
        /// The first chunk must be IHDR that contains Image width, height, bit depth, color type, compression method,
        /// filter method and interface (13 bytes)
        ///     Color type can be:
        ///         - 0 Grayscale
        ///         - 1
        ///         - 2 true color
        ///         - 3 indexed color
        ///         - 4 grayscale with alpha
        ///         - 5
        ///         - 6 true color with alpha
        /// PLTE contains the palette:
        ///     Color type (lower 3 bits):
        ///         - 0 grayscale
        ///         - 2 rgb
        ///         - 3 indexed
        ///         - 4 grayscale and alpha
        ///         - 6 rgba
        /// IDAT contains the image
        ///     It's preceded by a type byte:
        ///         - 0 None - No filtering
        ///         - 1 Sub - Byte A, Filtered to the left
        ///         - 2 Up - Byte B, above
        ///         - 3 Average - Mean of bytes A and B, rounded down
        ///         - 4 Paeth - A,B,C, whichever is closest to p = A + B - C
        /// IEND marks the image end
        /// bKGD sets the default background color
        /// cHRM gives the chromaticity coordinates of the display primaries and white point.
        /// gAMA specifies gamma.
        /// hIST can store the histogram, or total amount of each color in the image.
        /// iCCP is an ICC color profile.
        /// iTXt contains UTF-8 text, compressed or not, with an optional language tag.iTXt chunk with the keyword 'XML:com.adobe.xmp' can contain Extensible Metadata Platform (XMP).
        /// pHYs holds the intended pixel size and/or aspect ratio of the image.
        /// sBIT(significant bits) indicates the color-accuracy of the source data.
        /// sPLT suggests a palette to use if the full range of colors is unavailable.
        /// sRGB indicates that the standard sRGB color space is used.
        /// sTER stereo-image indicator chunk for stereoscopic images.[13]
        /// tEXt can store text that can be represented in ISO/IEC 8859-1, with one key-value pair for each chunk. The "key" must be between 1 and 79 characters long. Separator is a null character.The "value" can be any length, including zero up to the maximum permissible chunk size minus the length of the keyword and separator.Neither "key" nor "value" can contain null character.Leading or trailing spaces are also disallowed.
        /// tIME stores the time that the image was last changed.
        /// tRNS contains transparency information. For indexed images, it stores alpha channel values for one or more palette entries.For truecolor and grayscale images, it stores a single pixel value that is to be regarded as fully transparent.
        /// zTXt contains compressed text with the same limits as tEXt.
        /// </summary>
        public string Type { get; set; }
        public byte[] Data { get; set; }
        public int CRC { get; set; }
        
        public PngChunk(Stream stream)
        {
            var buffer = new byte[4];

            stream.Read(buffer, 0, buffer.Length);

            Length = BitConverter.ToInt32(buffer.Reverse().ToArray(), 0);

            stream.Read(buffer, 0, buffer.Length);

            Type = Encoding.ASCII.GetString(buffer);

            buffer = new byte[Length];

            stream.Read(buffer, 0, buffer.Length);

            Data = buffer;

            buffer = new byte[4];

            stream.Read(buffer, 0, buffer.Length);

            CRC = BitConverter.ToInt32(buffer, 0);
        }

        public static void Write(Stream stream, string type, byte[] data)
        {
            var buffer = new byte[4 + data.Length];
            
            Array.Copy(Encoding.ASCII.GetBytes(type), 0, buffer, 0, 4);
            Array.Copy(data, 0, buffer, 4, data.Length);

            var crc = buffer.Crc32(0, buffer.Length, 0);

            var length = BitConverter.GetBytes(data.Length).Reverse().ToArray();

            stream.Write(length, 0, length.Length);

            stream.Write(buffer, 0, buffer.Length);

            stream.Write(BitConverter.GetBytes(crc).Reverse().ToArray(), 0, 4);
        }

        public struct cHRMData
        {
            public uint WhitePointX { get; set; }
            public uint WhitePointY { get; set; }
            public uint RedX { get; set; }
            public uint RedY { get; set; }
            public uint GreenX { get; set; }
            public uint GreenY { get; set; }
            public uint BlueX { get; set; }
            public uint BlueY { get; set; }
        }

        public cHRMData cHRMInfo
        {
            get
            {
                if (Type != "cHRM")
                    throw new Exception("This is not a cHRM chunk.");

                return new cHRMData()
                {
                    WhitePointX = BitConverter.ToUInt32(Data, 0),
                    WhitePointY = BitConverter.ToUInt32(Data, 0),
                    RedX = BitConverter.ToUInt32(Data, 0),
                    RedY = BitConverter.ToUInt32(Data, 0),
                    GreenX = BitConverter.ToUInt32(Data, 0),
                    GreenY = BitConverter.ToUInt32(Data, 0),
                    BlueX = BitConverter.ToUInt32(Data, 0),
                    BlueY = BitConverter.ToUInt32(Data, 0)
                };
            }
        }

        /// <summary>
        /// returns a list of colors formated as rgb 1 byte per color values (24 bit colors)
        /// </summary>
        public List<byte[]> PLTEInfo
        {
            get
            {
                if (Type != "PLTE")
                    throw new Exception("This is not a PLTE chunk.");

                if (Data.Length % 3 != 0)
                    throw new Exception("Wrong PLTE chunk format");

                var result = new List<byte[]>();

                for (var i = 0; i < Data.Length / 3; i++)
                    result.Add(new byte[] { Data[i * 3], Data[i * 3 + 1], Data[i * 3 + 2] });

                return result;
            }
        }

        public IHDRData IRDRInfo
        {
            get
            {
                if (Type != "IHDR")
                    throw new Exception("This is not an IHDR chunk.");

                return new IHDRData()
                {
                    Width = BitConverter.ToInt32(Data.Take(4).Reverse().ToArray(), 0),
                    Height = BitConverter.ToInt32(Data.Skip(4).Take(4).Reverse().ToArray(), 0),
                    BitDepth = Data[8],
                    ColorType = (ColorTypeCode)Data[9],
                    CompressionMethod = (CompressionMethodCode)Data[10],
                    FilterMethod = (FilterTypeCode)Data[11],
                    InterlaceMethod = (InterlaceMethodCode)Data[12]
                };
            }
        }

        public static byte[] CreateIHDRData(IHDRData data)
        {
            var buffer = new List<byte>();

            buffer.AddRange(BitConverter.GetBytes(data.Width).Reverse());
            buffer.AddRange(BitConverter.GetBytes(data.Height).Reverse());
            buffer.Add(data.BitDepth);
            buffer.Add((byte)data.ColorType);
            buffer.Add((byte)data.CompressionMethod);
            buffer.Add((byte)data.FilterMethod);
            buffer.Add((byte)data.InterlaceMethod);

            return buffer.ToArray();
        }

        public struct IHDRData
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public byte BitDepth { get; set; }
            public ColorTypeCode ColorType { get; set; }
            /// <summary>
            /// Only method 0 defined (deflate)
            /// </summary>
            public CompressionMethodCode CompressionMethod { get; set; }
            public FilterTypeCode FilterMethod { get; set; }
            /// <summary>
            /// 0 for no interlace, 1 for Adam7 interlace
            /// </summary>
            public InterlaceMethodCode InterlaceMethod { get; set; }
        }

        public enum InterlaceMethodCode : byte
        {
            None = 0,
            Adam7 = 1
        }

        public enum CompressionMethodCode : byte
        {
            Deflate = 0
        }

        public enum FilterTypeCode : byte
        {
            None = 0,
            Sub = 1,
            Up = 2,
            Average = 3,
            Paeth = 4
        }

        public enum ColorTypeCode : byte
        {
            None = 0,
            ColorUsed = 2,
            PaletteUsedColorUsed = 3,
            AlphaChannelUsed = 4,
            ColorUsedAlphaChannelUsed = 6
        }

        public static bool CheckBitDepth(ColorTypeCode colorType, byte bitDepth)
        {
            if (colorType == ColorTypeCode.None)
            {
                if (bitDepth == 1 || bitDepth == 2 || bitDepth == 4 || bitDepth == 8 || bitDepth == 16)
                    return true; // each pixel is a grayscale sample
            }
            else if (colorType == ColorTypeCode.ColorUsed)
            {
                if (bitDepth == 8 || bitDepth == 16)
                    return true; // each pixel is an R, G, B triple
            }
            else if (colorType == ColorTypeCode.PaletteUsedColorUsed)
            {
                if (bitDepth == 1 || bitDepth == 2 || bitDepth == 4 || bitDepth == 8)
                    return true; // each pixel is a palette index, PLTE chunk required
            }
            else if (colorType == ColorTypeCode.AlphaChannelUsed)
            {
                if (bitDepth == 8 || bitDepth == 16)
                    return true; // each pixel is a grayscale sample followed by an alpha sample
            }
            else if (colorType == ColorTypeCode.ColorUsedAlphaChannelUsed)
            {
                if (bitDepth == 8 || bitDepth == 16)
                    return true; // each pixel is an R, G, B triple followed by an alpha sample
            }

            return false;
        }
    }
}
