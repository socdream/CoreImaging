using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreImaging.Tiff
{
    public class Tag
    {
        /// <summary>
        /// Tag identifying code
        /// </summary>
        public TagCode Code { get; set; }
        /// <summary>
        /// Datatype of tag data
        /// </summary>
        public TagType DataType { get; set; }
        /// <summary>
        /// Number of values
        /// </summary>
        public uint Count { get; set; }
        /// <summary>
        /// The Value Offset, the file offset (in bytes) of the Value for the field.
        /// The Value is expected to begin on a word boundary; the corresponding
        /// Value Offset will thus be an even number.This file offset may
        /// point anywhere in the file, even after the image data.
        /// </summary>
        public uint Value { get; set; }
        public object Data { get; set; }
        public int ByteCount
        {
            get
            {
                var header = 12;
                
                switch (DataType)
                {
                    case TagType.Byte:
                    case TagType.SByte:
                        if (Count <= 4)
                            return header + (int)Count;

                        return header + 4;
                    case TagType.Ascii:
                        if (Count <= 4)
                            return header + (int)Count;

                        return header + 4;
                    case TagType.Float:
                        if (Count * 4 <= 4)
                            return header + (int)Count * 4;

                        return header + 4;
                    case TagType.Long:
                    case TagType.SLong:
                        if (Count == 1)
                            return header + (int)Count * 4;

                            return header + 4;
                    case TagType.Rational:
                    case TagType.SRational:
                    case TagType.Double:
                        return header + 4;
                    case TagType.Short:
                    case TagType.SShort:
                        if (Count * 2 <= 4)
                            return header + (int)Count * 2;

                        return header + 4;
                }

                return header;
            }
        }
        public int DataSize
        {
            get
            {
                switch (DataType)
                {
                    case TagType.Byte:
                    case TagType.SByte:
                        if (Count <= 4)
                            return 0;

                        return (int)Count;
                    case TagType.Ascii:
                        if (Count <= 4)
                            return 0;

                        return (int)Count;
                    case TagType.Float:
                    case TagType.Long:
                    case TagType.SLong:
                        if (Count * 4 <= 4)
                            return 0;

                        return (int)Count * 4;
                    case TagType.Rational:
                    case TagType.SRational:
                    case TagType.Double:
                        return (int)Count * 8;
                    case TagType.Short:
                    case TagType.SShort:
                        if (Count * 2 <= 4)
                            return 0;

                        return (int)Count * 2;
                }

                return 0;
            }
        }

        public Tag()
        {

        }

        public Tag(Stream stream)
        {
            var buffer = new byte[12];

            stream.Read(buffer, 0, buffer.Length);

            Code = (TagCode)BitConverter.ToUInt16(buffer, 0);
            DataType = (TagType)BitConverter.ToInt16(buffer, 2);
            Count = BitConverter.ToUInt32(buffer, 4);
            Value = BitConverter.ToUInt32(buffer, 8);

            var pos = stream.Position;

            switch (DataType)
            {
                case TagType.Byte:
                case TagType.SByte:
                    if (Count == 1)
                        Data = (byte)Value;
                    else
                    {
                        if (Count > 4)
                            stream.Position = Value;
                        else
                            stream.Position -= 4;

                        buffer = new byte[Count];
                        stream.Read(buffer, 0, buffer.Length);
                        Data = buffer.Clone();
                    }
                    break;
                case TagType.Ascii:
                    stream.Position = Value;

                    buffer = new byte[Count];
                    stream.Read(buffer, 0, buffer.Length);
                    Data = System.Text.ASCIIEncoding.ASCII.GetString(buffer);
                    break;
                case TagType.Double:
                    stream.Position = Value;

                    buffer = new byte[Count * 8];
                    stream.Read(buffer, 0, buffer.Length);
                    Data = new double[Count];
                    Buffer.BlockCopy(buffer, 0, (double[])Data, 0, buffer.Length);
                    break;
                case TagType.Float:
                    if (Count == 1)
                        Data = (float)Value;
                    else
                    {
                        stream.Position = Value;

                        buffer = new byte[Count * 4];
                        stream.Read(buffer, 0, buffer.Length);
                        Data = new float[Count];
                        Buffer.BlockCopy(buffer, 0, (float[])Data, 0, buffer.Length);
                    }
                    break;
                case TagType.Long:
                    if (Count == 1)
                        Data = (uint)Value;
                    else
                    {
                        stream.Position = Value;

                        buffer = new byte[Count * 4];
                        stream.Read(buffer, 0, buffer.Length);
                        Data = new uint[Count];
                        Buffer.BlockCopy(buffer, 0, (uint[])Data, 0, buffer.Length);
                    }
                    break;
                case TagType.Rational:
                    stream.Position = Value;

                    buffer = new byte[Count * 4 * 2];
                    stream.Read(buffer, 0, buffer.Length);

                    for (int i = 0; i < Count; i++)
                    {
                        var numerator = BitConverter.ToUInt32(buffer, i * 4 * 2);
                        var denominator = BitConverter.ToUInt32(buffer, i * 4 * 2 + 4);

                        Data = (float)numerator / denominator;
                    }
                    break;
                case TagType.Short:
                    if (Count == 1)
                        Data = (ushort)Value;
                    else
                    {
                        if (Count > 2)
                            stream.Position = Value;

                        buffer = new byte[Count * 2];
                        stream.Read(buffer, 0, buffer.Length);
                        Data = new ushort[Count];
                        Buffer.BlockCopy(buffer, 0, (ushort[])Data, 0, buffer.Length);
                    }
                    break;
                case TagType.SLong:
                    if (Count == 1)
                        Data = (int)Value;
                    else
                    {
                        stream.Position = Value;

                        buffer = new byte[Count * 4];
                        stream.Read(buffer, 0, buffer.Length);
                        Data = new int[Count];
                        Buffer.BlockCopy(buffer, 0, (int[])Data, 0, buffer.Length);
                    }
                    break;
                case TagType.SRational:
                    stream.Position = Value;

                    buffer = new byte[Count * 4 * 2];
                    stream.Read(buffer, 0, buffer.Length);

                    for (int i = 0; i < Count; i++)
                    {
                        var numerator = BitConverter.ToInt32(buffer, i * 4 * 2);
                        var denominator = BitConverter.ToInt32(buffer, i * 4 * 2 + 4);

                        Data = (float)numerator / denominator;
                    }
                    break;
                case TagType.SShort:
                    if (Count == 1)
                        Data = (short)Value;
                    else
                    {
                        if (Count > 2)
                            stream.Position = Value;

                        buffer = new byte[Count * 2];
                        stream.Read(buffer, 0, buffer.Length);
                        Data = new short[Count];
                        Buffer.BlockCopy(buffer, 0, (short[])Data, 0, buffer.Length);
                    }
                    break;
                case TagType.Undefined:
                    Data = Value;
                    break;
            }

            stream.Position = pos;
        }
        
        public void Write(Stream stream, int dataOffset)
        {
            var buffer = BitConverter.GetBytes((ushort)Code);

            stream.Write(buffer, 0, buffer.Length);

            buffer = BitConverter.GetBytes((short)DataType);

            stream.Write(buffer, 0, buffer.Length);

            buffer = BitConverter.GetBytes(Count);

            stream.Write(buffer, 0, buffer.Length);
            
            WriteTagData(stream, dataOffset);
        }

        private void WriteTagData(Stream stream, int dataOffset)
        {
            var data = GetWriteData();
            var pos = stream.Position + 4;

            if (data.Length > 4)
            {
                var offsetBuffer = BitConverter.GetBytes(dataOffset);
                stream.Write(offsetBuffer, 0, offsetBuffer.Length);

                if(stream.Length < dataOffset - 1)
                {
                    var addedBytes = new byte[dataOffset - stream.Length];

                    stream.Position = stream.Length - 1;
                    stream.Write(addedBytes, 0, addedBytes.Length);
                }

                stream.Position = dataOffset;
            }

            stream.Write(data, 0, data.Length);

            stream.Position = pos;
        }

        private byte[] GetWriteData()
        {
            var data = new byte[0];

            switch (DataType)
            {
                case TagType.Byte:
                    if (Count == 1)
                        data = new byte[] { (byte)Value };
                    else
                    {
                        data = (byte[])Data;
                    }
                    break;
                case TagType.Ascii:
                    data = Encoding.ASCII.GetBytes((string)Data);
                    break;
                case TagType.Short:
                    if (Count == 1)
                        data = BitConverter.GetBytes((ushort)Value);
                    else
                    {
                        data = new byte[Count * 2];
                        Buffer.BlockCopy((ushort[])Data, 0, data, 0, (int)Count * 2);
                    }
                    break;
                case TagType.Long:
                    if (Count == 1)
                        data = BitConverter.GetBytes((ushort)Value);
                    else
                    {
                        data = new byte[Count * 4];
                        Buffer.BlockCopy((uint[])Data, 0, data, 0, data.Length);
                    }
                    break;
                case TagType.Rational:
                    data = new byte[8];
                    var numerator = BitConverter.GetBytes((uint)(float)Data);
                    var denominator = BitConverter.GetBytes((uint)1);
                    Array.Copy(numerator, data, numerator.Length);
                    Array.Copy(denominator, 0, data, 4, 4);
                    break;
                case TagType.SByte:
                    if (Count == 1)
                        data = new byte[] { (byte)Value };
                    else
                    {
                        data = (byte[])Data;
                    }
                    break;
                case TagType.Undefined:
                    break;
                case TagType.SShort:
                    if (Count == 1)
                        data = BitConverter.GetBytes((short)Value);
                    else
                    {
                        data = new byte[Count * 2];
                        Buffer.BlockCopy((short[])Data, 0, data, 0, (int)Count * 2);
                    }
                    break;
                case TagType.SLong:
                    if (Count == 1)
                        data = BitConverter.GetBytes((ushort)Value);
                    else
                    {
                        data = new byte[Count * 4];
                        Buffer.BlockCopy((uint[])Data, 0, data, 0, data.Length);
                    }
                    break;
                case TagType.SRational:
                    data = new byte[8];
                    var numeratorSigned = BitConverter.GetBytes((int)(float)Data);
                    var denominatorSigned = BitConverter.GetBytes((int)1);
                    Array.Copy(numeratorSigned, data, numeratorSigned.Length);
                    Array.Copy(denominatorSigned, 0, data, 4, 4);
                    break;
                case TagType.Float:
                    if (Count == 1)
                        data = BitConverter.GetBytes((float)Value);
                    else
                    {
                        data = new byte[Count * 4];
                        Buffer.BlockCopy((float[])Data, 0, data, 0, data.Length);
                    }
                    break;
                case TagType.Double:
                    if (Count == 1)
                        data = BitConverter.GetBytes((double)Value);
                    else
                    {
                        data = new byte[Count * 8];
                        Buffer.BlockCopy((double[])Data, 0, data, 0, data.Length);
                    }
                    break;
                default:
                    break;
            }

            return data;
        }

        public enum TagCode : ushort
        {
            /*************************  Bilevel image  *******************************************/

            /// <summary>
            /// A bilevel image contains two colors—black and white. TIFF allows an application
            /// to write out bilevel data in either a white-is-zero or black-is-zero format. The
            /// field that records this information is called PhotometricInterpretation.
            /// 
            /// 0 = WhiteIsZero. For bilevel and grayscale images: 0 is imaged as white. The maximum
            /// value is imaged as black. This is the normal value for Compression = 2.
            /// 
            /// 1 = BlackIsZero. For bilevel and grayscale images: 0 is imaged as black. The maximum
            /// value is imaged as white. If this value is specified for Compression = 2, the
            /// image should display and print reversed.
            /// 
            /// 2 = RGB.  In the RGB model, a color is described as a combination of the three primary
            /// colors of light (red, green, and blue) in particular concentrations. For each of
            /// the three components, 0 represents minimum intensity, and 2**BitsPerSample - 1
            /// represents maximum intensity. Thus an RGB value of (0,0,0) represents black,
            /// and (255,255,255) represents white, assuming 8-bit components. For
            /// PlanarConfiguration = 1, the components are stored in the indicated order: first
            /// Red, then Green, then Blue. For PlanarConfiguration = 2, the StripOffsets for the
            /// component planes are stored in the indicated order: first the Red component plane
            /// StripOffsets, then the Green plane StripOffsets, then the Blue plane StripOffsets.
            /// 
            /// 3 = Palette Color. In this model, a color is described with a single component. The
            /// value of the component is used as an index into the red, green and blue curves in
            /// the ColorMap field to retrieve an RGB triplet that defines the color. When
            /// PhotometricInterpretation = 3 is used, ColorMap must be present and
            /// SamplesPerPixel must be 1.
            /// 
            /// 4 = Transparency Mask.
            /// This means that the image is used to define an irregularly shaped region of another
            /// image in the same TIFF file. SamplesPerPixel and BitsPerSample must be 1.
            /// PackBits compression is recommended. The 1-bits define the interior of the region;
            /// the 0-bits define the exterior of the region.
            /// A reader application can use the mask to determine which parts of the image to
            /// display. Main image pixels that correspond to 1-bits in the transparency mask are
            /// imaged to the screen or printer, but main image pixels that correspond to 0-bits in
            /// the mask are not displayed or printed.
            /// 
            /// 5 = Separated - usually CMYK
            /// The components represent the desired percent dot coverage of each ink, where
            /// the larger component values represent a higher percentage of ink dot coverage
            /// and smaller values represent less coverage.
            /// </summary>
            PhotometricInterpretation = 262,
            /// <summary>
            /// Data can be stored either compressed or uncompressed
            /// 
            /// 1 = No compression, but pack data into bytes as tightly as possible, leaving no unused
            /// bits (except at the end of a row). The component values are stored as an array of
            /// type BYTE. Each scan line (row) is padded to the next BYTE boundary.
            /// 
            /// 2 = CCITT Group 3 1-Dimensional Modified Huffman run length encoding
            /// 
            /// 3 = CCITT Group 3 modified, lines start on bit boundary, line end marked with EOL flag
            /// 
            /// 4 = CCITT Group 4 line difference encoding
            /// 
            /// 5 = LZW
            /// 
            /// 32773 = PackBits compression, a simple byte-oriented run length scheme. See the
            /// PackBits section for details.
            /// Data compression applies only to raster image data. All other TIFF fields are
            /// unaffected.
            /// </summary>
            Compression = 259,
            /// <summary>
            /// The number of rows (sometimes described as scanlines) in the image
            /// </summary>
            ImageLength = 257,
            /// <summary>
            /// The number of columns in the image, i.e., the number of pixels per scanline.
            /// </summary>
            ImageWidth = 256,
            /// <summary>
            /// Applications often want to know the size of the picture represented by an image.
            /// This information can be calculated from ImageWidth and ImageLength given the
            /// following resolution data:
            /// 
            /// 1 = No absolute unit of measurement. Used for images that may have a non-square
            /// aspect ratio but no meaningful absolute dimensions.
            /// 2 = Inch.
            /// 3 = Centimeter.
            /// </summary>
            ResolutionUnit = 296,
            /// <summary>
            /// The number of pixels per ResolutionUnit in the ImageWidth (typically, horizontal
            /// - see Orientation) direction
            /// </summary>
            XResolution = 282,
            /// <summary>
            /// The number of pixels per ResolutionUnit in the ImageLength (typically, vertical)
            /// direction.
            /// </summary>
            YResolution = 283,
            /// <summary>
            /// The number of rows in each strip (except possibly the last strip.)
            /// For example, if ImageLength is 24, and RowsPerStrip is 10, then there are 3
            /// strips, with 10 rows in the first strip, 10 rows in the second strip, and 4 rows in the
            /// third strip. (The data in the last strip is not padded with 6 extra rows of dummy
            /// data.)
            /// </summary>
            RowsPerStrip = 278,
            /// <summary>
            /// For each strip, the byte offset of that strip.
            /// </summary>
            StripOffsets = 273,
            /// <summary>
            /// For each strip, the number of bytes in that strip after any compression
            /// </summary>
            StripByteCounts = 279,

            /*****************************  Grayscale  ********************************************/

            /// <summary>
            /// The number of bits per component.
            /// Allowable values for Baseline TIFF grayscale images are 4 and 8, allowing either
            /// 16 or 256 distinct shades of gray.
            /// </summary>
            BitsPerSample = 258,

            /****************************   Palette-color   ***********************************/

            /// <summary>
            /// This field defines a Red-Green-Blue color map (often called a lookup table) for
            /// palette color images. In a palette-color image, a pixel value is used to index into an
            /// RGB-lookup table. For example, a palette-color pixel having a value of 0 would
            /// be displayed according to the 0th Red, Green, Blue triplet.
            /// In a TIFF ColorMap, all the Red values come first, followed by the Green values,
            /// then the Blue values. In the ColorMap, black is represented by 0,0,0 and white is
            /// represented by 65535, 65535, 65535.
            /// </summary>
            ColorMap = 320,

            /***************************  RGB Full Color  **************************************/

            /// <summary>
            /// The number of components per pixel. This number is 3 for RGB images, unless
            /// extra samples are present. See the ExtraSamples field for further information.
            /// </summary>
            SamplesPerPixel = 277,


            /*************************** Base Line fields  ****************************************/

            /// <summary>
            /// Person who created the image.
            /// </summary>
            Artist = 315,
            /// <summary>
            /// The length of the dithering or halftoning matrix used to create a dithered or
            /// halftoned bilevel file.
            /// 
            /// This field should only be present if Threshholding = 2
            /// </summary>
            CellLength = 265,
            /// <summary>
            /// The width of the dithering or halftoning matrix used to create a dithered or
            /// halftoned bilevel file.Tag = 264(108.H)
            /// </summary>
            CellWidth = 264,
            /// <summary>
            /// Copyright notice of the person or organization that claims the copyright to the
            /// image. The complete copyright statement should be listed in this field including
            /// any dates and statements of claims. For example, “Copyright, John Smith, 19xx.
            /// All rights reserved.”
            /// </summary>
            Copyright = 33432,
            /// <summary>
            /// The format is: “YYYY:MM:DD HH:MM:SS”, with hours like those on a 24-hour
            /// clock, and one space character between the date and the time. The length of the
            /// string, including the terminating NUL, is 20 bytes.
            /// </summary>
            DateTime = 306,
            /// <summary>
            /// Specifies that each pixel has m extra components whose interpretation is defined
            /// by one of the values listed below. When this field is used, the SamplesPerPixel
            /// field has a value greater than the PhotometricInterpretation field suggests.
            /// For example, full-color RGB data normally has SamplesPerPixel = 3.If
            /// SamplesPerPixel is greater than 3, then the ExtraSamples field describes the
            /// meaning of the extra samples. If SamplesPerPixel is, say, 5 then ExtraSamples
            /// will contain 2 values, one for each extra sample.
            /// ExtraSamples is typically used to include non-color information, such as opacity,
            /// in an image. The possible values for each item in the field's value are:
            /// 0 = Unspecified data
            /// 1 = Associated alpha data (with pre-multiplied color)
            /// 2 = Unassociated alpha data
            /// Associated alpha data is opacity information; it is fully described in Section 21.
            /// Unassociated alpha data is transparency information that logically exists independent
            /// of an image; it is commonly called a soft matte. Note that including both
            /// unassociated and associated alpha is undefined because associated alpha specifies
            /// that color components are pre-multiplied by the alpha component, while
            /// unassociated alpha specifies the opposite.
            /// By convention, extra components that are present must be stored as the “last components”
            /// in each pixel. For example, if SamplesPerPixel is 4 and there is 1 extra
            /// component, then it is located in the last component location (SamplesPerPixel-1)
            /// in each pixel.
            /// Components designated as “extra” are just like other components in a pixel. In
            /// particular, the size of such components is defined by the value of the
            /// BitsPerSample field.
            /// With the introduction of this field, TIFF readers must not assume a particular
            /// SamplesPerPixel value based on the value of the PhotometricInterpretation field.
            /// For example, if the file is an RGB file, SamplesPerPixel may be greater than 3.
            /// The default is no extra samples. This field must be present if there are extra
            /// samples.
            /// See also SamplesPerPixel, AssociatedAlpha
            /// </summary>
            ExtraSamples = 338,
            /// <summary>
            /// 1 = pixels are arranged within a byte such that pixels with lower column values are
            /// stored in the higher-order bits of the byte.
            /// 1-bit uncompressed data example: Pixel 0 of a row is stored in the high-order bit
            /// of byte 0, pixel 1 is stored in the next-highest bit, ..., pixel 7 is stored in the loworder
            /// bit of byte 0, pixel 8 is stored in the high-order bit of byte 1, and so on.
            /// CCITT 1-bit compressed data example: The high-order bit of the first compression
            /// code is stored in the high-order bit of byte 0, the next-highest bit of the first
            /// compression code is stored in the next-highest bit of byte 0, and so on.
            /// 2 = pixels are arranged within a byte such that pixels with lower column values are
            /// stored in the lower-order bits of the byte.
            /// We recommend that FillOrder = 2 be used only in special-purpose applications.It
            /// is easy and inexpensive for writers to reverse bit order by using a 256-byte lookup
            /// table.FillOrder = 2 should be used only when BitsPerSample = 1 and the data is
            /// either uncompressed or compressed using CCITT 1D or 2D compression, to
            /// avoid potentially ambigous situations.
            /// Support for FillOrder =2 is not required in a Baseline TIFF compliant reader
            /// Default is FillOrder = 1.
            /// </summary>
            FillOrder = 266,
            /// <summary>
            /// For each string of contiguous unused bytes in a TIFF file, the number of bytes in
            /// the string
            /// </summary>
            FreeByteCounts = 289,
            /// <summary>
            /// For each string of contiguous unused bytes in a TIFF file, the byte offset of the
            /// string.
            /// </summary>
            FreeOffsets =288,
            /// <summary>
            /// For grayscale data, the optical density of each possible pixel value.
            /// The 0th value of GrayResponseCurve corresponds to the optical density of a pixel
            /// having a value of 0, and so on.
            /// This field may provide useful information for sophisticated applications, but it is
            /// currently ignored by most TIFF readers.
            /// See also GrayResponseUnit, PhotometricInterpretation.
            /// </summary>
            GrayResponseCurve = 291,
            /// <summary>
            /// The precision of the information contained in the GrayResponseCurve.
            /// Because optical density is specified in terms of fractional numbers, this field is
            /// necessary to interpret the stored integer information. For example, if
            /// GrayScaleResponseUnits is set to 4 (ten-thousandths of a unit), and a
            /// GrayScaleResponseCurve number for gray level 4 is 3455, then the resulting
            /// actual value is 0.3455.
            /// Optical densitometers typically measure densities within the range of 0.0 to 2.0
            /// 
            /// 1 = Number represents tenths of a unit.
            /// 2 = Number represents hundredths of a unit.
            /// 3 = Number represents thousandths of a unit.
            /// 4 = Number represents ten-thousandths of a unit.
            /// 5 = Number represents hundred-thousandths of a unit.
            /// Modifies GrayResponseCurve.
            /// See also GrayResponseCurve.
            /// For historical reasons, the default is 2. However, for greater accuracy, 3 is recommended.
            /// </summary>
            GrayResponseUnit = 290,
            /// <summary>
            /// The computer and/or operating system in use at the time of image creation.
            /// </summary>
            HostComputer = 316,
            /// <summary>
            /// A string that describes the subject of the image.
            /// </summary>
            ImageDescription = 270,
            /// <summary>
            /// The scanner manufacturer.
            /// </summary>
            Make = 271,
            /// <summary>
            /// The maximum component value used.
            /// 
            /// This field is not to be used to affect the visual appearance of an image when it is
            /// displayed or printed. Nor should this field affect the interpretation of any other
            /// field; it is used only for statistical purposes.
            /// Default is 2**(BitsPerSample) - 1.
            /// </summary>
            MaxSampleValue = 281,
            /// <summary>
            /// The minimum component value used.
            /// 
            /// Default is 0
            /// </summary>
            MinSampleValue = 280,
            /// <summary>
            /// The scanner model name or number
            /// </summary>
            Model = 272,
            /// <summary>
            /// A general indication of the kind of data contained in this subfile.
            /// Tag = 254(FE.H)
            /// Type = LONG
            /// N = 1
            /// Replaces the old SubfileType field, due to limitations in the definition of that field.
            /// NewSubfileType is mainly useful when there are multiple subfiles in a single
            /// TIFF file.
            /// This field is made up of a set of 32 flag bits. Unused bits are expected to be 0. Bit 0
            /// is the low-order bit.
            /// Currently defined values are:
            /// Bit 0 is 1 if the image is a reduced-resolution version of another image in this TIFF file;
            /// else the bit is 0.
            /// Bit 1 is 1 if the image is a single page of a multi-page image (see the PageNumber field
            /// description); else the bit is 0.
            /// Bit 2 is 1 if the image defines a transparency mask for another image in this TIFF file.
            /// The PhotometricInterpretation value must be 4, designating a transparency mask.
            /// These values are defined as bit flags because they are independent of each other.
            /// Default is 0.
            /// </summary>
            NewSubfileType = 254,
            /// <summary>
            /// The orientation of the image with respect to the rows and columns
            /// 
            /// 1 = The 0th row represents the visual top of the image, and the 0th column represents
            /// the visual left-hand side.
            /// 2 = The 0th row represents the visual top of the image, and the 0th column represents
            /// the visual right-hand side.
            /// 3 = The 0th row represents the visual bottom of the image, and the 0th column represents
            /// the visual right-hand side.
            /// 4 = The 0th row represents the visual bottom of the image, and the 0th column represents
            /// the visual left-hand side.
            /// 5 = The 0th row represents the visual left-hand side of the image, and the 0th column
            /// represents the visual top.
            /// 6 = The 0th row represents the visual right-hand side of the image, and the 0th column
            /// represents the visual top.
            /// 7 = The 0th row represents the visual right-hand side of the image, and the 0th column
            /// represents the visual bottom.
            /// 8 = The 0th row represents the visual left-hand side of the image, and the 0th column
            /// represents the visual bottom.
            /// Default is 1.
            /// </summary>
            Orientation = 274,
            /// <summary>
            /// How the components of each pixel are stored.
            /// 
            /// 1 = Chunky format. The component values for each pixel are stored contiguously.
            /// The order of the components within the pixel is specified by
            /// PhotometricInterpretation. For example, for RGB data, the data is stored as
            /// RGBRGBRGB…
            /// 2 = Planar format. The components are stored in separate “component planes.” The
            /// values in StripOffsets and StripByteCounts are then arranged as a 2-dimensional
            /// array, with SamplesPerPixel rows and StripsPerImage columns. (All of the columns
            /// for row 0 are stored first, followed by the columns of row 1, and so on.)
            /// PhotometricInterpretation describes the type of data stored in each component
            /// plane. For example, RGB data is stored with the Red components in one component
            /// plane, the Green in another, and the Blue in another.
            /// PlanarConfiguration = 2 is not currently in widespread use and it is not recommended
            /// for general interchange. It is used as an extension and Baseline TIFF
            /// readers are not required to support it.
            /// If SamplesPerPixel is 1, PlanarConfiguration is irrelevant, and need not be included.
            /// If a row interleave effect is desired, a writer might write out the data as
            /// PlanarConfiguration = 2—separate sample planes—but break up the planes into
            /// multiple strips (one row per strip, perhaps) and interleave the strips.
            /// </summary>
            PlanarConfiguration = 284,
            /// <summary>
            /// Name and version number of the software package(s) used to create the image
            /// </summary>
            Software = 305,
            /// <summary>
            /// A general indication of the kind of data contained in this subfile.
            /// 
            /// Currently defined values are:
            /// 1 = full-resolution image data
            /// 2 = reduced-resolution image data
            /// 3 = a single page of a multi-page image (see the PageNumber field description).
            /// Note that several image types may be found in a single TIFF file, with each subfile
            /// described by its own IFD.
            /// No default.
            /// This field is deprecated. The NewSubfileType field should be used instead
            /// </summary>
            SubfileType = 255,
            /// <summary>
            /// For black and white TIFF files that represent shades of gray, the technique used to
            /// convert from gray to black and white pixels.
            /// 
            /// 1 = No dithering or halftoning has been applied to the image data.
            /// 2 = An ordered dither or halftone technique has been applied to the image data.
            /// 3 = A randomized process such as error diffusion has been applied to the image data.
            /// Default is Threshholding = 1.See also CellWidth, CellLength.
            /// </summary>
            Threshholding = 263,
            /// <summary>
            /// XML packet containing XMP metadata
            /// </summary>
            XMP = 700,
            /// <summary>
            /// OPI-related.
            /// </summary>
            ImageID = 32781,
            /// <summary>
            /// Defined in the Mixed Raster Content part of RFC 2301, used to denote the particular function of this Image in the mixed raster scheme.
            /// </summary>
            ImageLayer = 34732,
            /// <summary>
            /// Collection of Photoshop 'Image Resource Blocks'.
            /// </summary>
            Photoshop = 34377,
            /// <summary>
            /// A pointer to the Exif IFD
            /// </summary>
            ExifIFD = 34665,
            /// <summary>
            /// Used by Adobe Photoshop.
            /// </summary>
            ImageSourceData = 37724,
            /// <summary>
            /// IPTC (International Press Telecommunications Council) metadata.
            /// </summary>
            IPTC = 33723
        }

        public enum TagType :short
       {
           /// <summary>
           /// 8-bit unsigned integer.
           /// </summary>
           Byte = 1,
           /// <summary>
           /// 8-bit byte that contains a 7-bit ASCII code; the last byte
           /// must be NUL (binary zero).
           /// </summary>
           Ascii = 2,
           /// <summary>
           /// 16-bit (2-byte) unsigned integer.
           /// </summary>
           Short = 3,
           /// <summary>
           /// 32-bit (4-byte) unsigned integer
           /// </summary>
           Long = 4,
           /// <summary>
           ///  Two LONGs: the first represents the numerator of a
           /// fraction; the second, the denominator.
           /// </summary>
           Rational = 5,
           /// <summary>
           /// An 8-bit signed (twos-complement) integer.
           /// </summary>
           SByte = 6,
           /// <summary>
           /// An 8-bit byte that may contain anything, depending on
           /// the definition of the field.
           /// </summary>
           Undefined = 7,
           /// <summary>
           /// A 16-bit (2-byte) signed (twos-complement) integer
           /// </summary>
           SShort = 8,
           /// <summary>
           /// A 32-bit (4-byte) signed (twos-complement) integer
           /// </summary>
           SLong = 9,
           /// <summary>
           /// Two SLONG’s: the first represents the numerator of a
           /// fraction, the second the denominator.
           /// </summary>
           SRational = 10,
           /// <summary>
           /// Single precision (4-byte) IEEE format
           /// </summary>
           Float = 11,
           /// <summary>
           /// Double precision (8-byte) IEEE format
           /// </summary>
           Double = 12
       }
        public enum CompressionType : ushort
        {
            None = 1,
            ModifiedHuffman = 2,
            PackBits = 3
        }
   }
}
