using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreImaging.Tiff
{
    public class TiffImage : Image
    {
        public FileHeader Header { get; set; }
        public ImageFileDirectory[] IFDs { get; set; }
        public int Channels { get; set; }
        
        public int HeaderSize
        {
            get
            {
                var total = 8;

                foreach (var item in IFDs)
                    total += item.ByteCount;

                return total;
            }
        }

        public TiffImage(Image image)
        {
            DataStructure = image.DataStructure;
            Width = image.Width;
            Height = image.Height;
            Transforms = image.Transforms;
            Data = image.Data;
        }

        public TiffImage(string path)
        {
            using (var stream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                Header = new FileHeader(stream);

                var offset = Header.Offset;
                var ifds = new List<ImageFileDirectory>();

                while (offset != 0)
                {
                    stream.Position = offset;

                    var newIfd = new ImageFileDirectory(stream);

                    offset = newIfd.Offset;

                    ifds.Add(newIfd);
                }

                IFDs = ifds.ToArray();

                var photometricInterpretation = (PhotometricInterpretation)(ushort)ifds[0].Tags.Where(a => a.Code == Tag.TagCode.PhotometricInterpretation).FirstOrDefault().Data;

                switch (photometricInterpretation)
                {
                    case PhotometricInterpretation.RGB:
                        ReadRGB(stream);
                        break;
                    case PhotometricInterpretation.Separated:
                        var samples = ifds[0].Tags.FirstOrDefault(a => a.Code == Tag.TagCode.SamplesPerPixel);

                        if (samples == null)
                            throw new ArgumentException("The given files does not specify a number of samples per pixel.");

                        Channels = (int)samples.Value;

                        ReadSeparated(stream);
                        break;
                }
            }
        }

        private void ReadSeparated(Stream stream)
        {
            if (Channels != 4)
                throw new NotSupportedException("Images with more than 4 channels are not currently supported.");

            _dataStructure = ImageDataStructure.Cmyk;

            var planarConfiguration = (PlanarConfiguration)(ushort)IFDs[0].Tags.FirstOrDefault(a => a.Code == Tag.TagCode.PlanarConfiguration)?.Value;

            if (planarConfiguration == PlanarConfiguration.Chunky)
            {
                var offsets = IFDs[0].Tags.Where(a => a.Code == Tag.TagCode.StripOffsets).FirstOrDefault();
                var rowsPerStrip = (ushort)IFDs[0].Tags.Where(a => a.Code == Tag.TagCode.RowsPerStrip).FirstOrDefault().Data;
                Width = (ushort)IFDs[0].Tags.Where(a => a.Code == Tag.TagCode.ImageWidth).FirstOrDefault().Data;
                Height = (ushort)IFDs[0].Tags.Where(a => a.Code == Tag.TagCode.ImageLength).FirstOrDefault().Data;

                Data = new byte[Width * Height * Channels];

                var remainingLength = Height;
                var currentStrip = 0;

                while (remainingLength > 0)
                {
                    stream.Position = (offsets.Count == 1) ?
                        (offsets.DataType == Tag.TagType.Short) ?
                            (ushort)offsets.Data
                            : (uint)offsets.Data
                        : (offsets.DataType == Tag.TagType.Short) ?
                            ((ushort[])offsets.Data)[currentStrip]
                            : ((uint[])offsets.Data)[currentStrip];

                    stream.Read(Data, currentStrip * rowsPerStrip * Width * Channels, rowsPerStrip * Width * Channels);

                    remainingLength -= rowsPerStrip;
                }
            }
        }

        /// <summary>
        /// In the RGB model, a color is described as a combination of the three primary
        /// colors of light(red, green, and blue) in particular concentrations.For each of
        /// the three components, 0 represents minimum intensity, and 2**BitsPerSample - 1
        /// represents maximum intensity.Thus an RGB value of (0,0,0) represents black,
        /// and (255, 255, 255) represents white, assuming 8-bit components.For
        /// PlanarConfiguration = 1, the components are stored in the indicated order: first
        /// Red, then Green, then Blue. For PlanarConfiguration = 2, the StripOffsets for the
        /// component planes are stored in the indicated order: first the Red component plane
        /// StripOffsets, then the Green plane StripOffsets, then the Blue plane StripOffsets.
        /// </summary>
        /// <param name="stream"></param>
        private void ReadRGB(Stream stream)
        {
            _dataStructure = ImageDataStructure.Rgb;

            var planarConfiguration = (PlanarConfiguration)(ushort)IFDs[0].Tags.FirstOrDefault(a => a.Code == Tag.TagCode.PlanarConfiguration)?.Value;

            if(planarConfiguration == PlanarConfiguration.Chunky)
            {
                var offsets = IFDs[0].Tags.Where(a => a.Code == Tag.TagCode.StripOffsets).FirstOrDefault();
                var rowsPerStrip = (ushort)IFDs[0].Tags.Where(a => a.Code == Tag.TagCode.RowsPerStrip).FirstOrDefault().Data;
                Width = (ushort)IFDs[0].Tags.Where(a => a.Code == Tag.TagCode.ImageWidth).FirstOrDefault().Data;
                Height = (ushort)IFDs[0].Tags.Where(a => a.Code == Tag.TagCode.ImageLength).FirstOrDefault().Data;

                Data = new byte[Width * Height * 3];

                var remainingLength = Height;
                var currentStrip = 0;

                while(remainingLength > 0)
                {
                    stream.Position = (offsets.Count == 1) ?
                        (offsets.DataType == Tag.TagType.Short) ?
                            (ushort)offsets.Data
                            : (uint)offsets.Data
                        : (offsets.DataType == Tag.TagType.Short) ?
                            ((ushort[])offsets.Data)[currentStrip]
                            : ((uint[])offsets.Data)[currentStrip];

                    stream.Read(Data, currentStrip * rowsPerStrip * Width * 3, rowsPerStrip * Width * 3);

                    remainingLength -= rowsPerStrip;
                }
            }
        }

        public bool Save(string path)
        {
            if (DataStructure == ImageDataStructure.Rgb)
                return SaveRGB(path);

            return false;
        }

        private bool SaveRGB(string path)
        {
            try
            {
                DataStructure = ImageDataStructure.Rgb;

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    var header = new FileHeader()
                    {
                        ByteOrder = FileHeader.TiffByteOrder.II,
                        Offset = 8
                    };

                    header.Write(stream);

                    var ifd = new ImageFileDirectory()
                    {
                        Tags = new List<Tag>()
                        {
                            new Tag()
                            {
                                Code = Tag.TagCode.NewSubfileType,
                                Data = 0,
                                DataType = Tag.TagType.Long,
                                Count = 1,
                                Value = 0
                            },
                            new Tag()
                            {
                                Code = Tag.TagCode.ImageWidth,
                                Data = (ushort)Width,
                                DataType = Tag.TagType.Short,
                                Count = 1,
                                Value = (ushort)Width
                            },
                            new Tag()
                            {
                                Code = Tag.TagCode.ImageLength,
                                Data = (ushort)Height,
                                DataType = Tag.TagType.Short,
                                Count = 1,
                                Value = (ushort)Height
                            },
                            new Tag()
                            {
                                Code = Tag.TagCode.BitsPerSample,
                                Data = new ushort[] { 8, 8, 8 },
                                DataType = Tag.TagType.Short,
                                Count = 3,
                                Value = 0
                            },
                            new Tag()
                            {
                                Code = Tag.TagCode.Compression,
                                Data = (ushort)Tag.CompressionType.None,
                                DataType = Tag.TagType.Short,
                                Count = 1,
                                Value = (uint)Tag.CompressionType.None
                            },
                            new Tag()
                            {
                                Code = Tag.TagCode.PhotometricInterpretation,
                                Data = PhotometricInterpretation.RGB,
                                DataType = Tag.TagType.Short,
                                Count = 1,
                                Value = (uint)PhotometricInterpretation.RGB
                            },
                            new Tag()
                            {
                                Code = Tag.TagCode.StripOffsets,
                                Data = 0,
                                DataType = Tag.TagType.Long,
                                Count = 1,
                                Value = 0
                            },
                            new Tag()
                            {
                                Code = Tag.TagCode.Orientation,
                                Data = (ushort)1,
                                DataType = Tag.TagType.Short,
                                Count = 1,
                                Value = 1
                            },
                            new Tag()
                            {
                                Code = Tag.TagCode.SamplesPerPixel,
                                Count = 1,
                                Data = (ushort)3,
                                DataType = Tag.TagType.Short,
                                Value = 3
                            },
                            new Tag()
                            {
                                Code = Tag.TagCode.RowsPerStrip,
                                Data = (ushort)Height,
                                DataType = Tag.TagType.Short,
                                Count = 1,
                                Value = (ushort)Height
                            },
                            new Tag()
                            {
                                Code = Tag.TagCode.StripByteCounts,
                                Data = (uint)Height * 3,
                                DataType = Tag.TagType.Long,
                                Count = 1,
                                Value = (uint)Height * 3
                            },
                            new Tag()
                            {
                                Code = Tag.TagCode.XResolution,
                                Count = 1,
                                Data = 360f,
                                DataType = Tag.TagType.Rational
                            },
                            new Tag()
                            {
                                Code = Tag.TagCode.YResolution,
                                Count = 1,
                                Data = 360f,
                                DataType = Tag.TagType.Rational
                            },
                            new Tag()
                            {
                                Code = Tag.TagCode.PlanarConfiguration,
                                Data = PlanarConfiguration.Chunky,
                                DataType = Tag.TagType.Short,
                                Count = 1,
                                Value = (uint)PlanarConfiguration.Chunky
                            },
                            new Tag()
                            {
                                Code = Tag.TagCode.ResolutionUnit,
                                Count = 1,
                                Data = (ushort)2,
                                DataType = Tag.TagType.Short,
                                Value = (ushort)2
                            }
                        }
                    };

                    var tagsOffset = header.ByteCount + ifd.ByteCount;
                    var tagDataCount = 0;

                    foreach (var item in ifd.Tags)
                        tagDataCount += item.DataSize;

                    var stripsTag = ifd.Tags.FirstOrDefault(a => a.Code == Tag.TagCode.StripOffsets);

                    stripsTag.Value = (uint)(tagsOffset + tagDataCount);

                    ifd.Write(stream, tagsOffset);

                    stream.Write(Data, 0, Data.Length);
                }

            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public enum PlanarConfiguration : ushort
        {
            /// <summary>
            /// The component values for each pixel are stored contiguously.
            /// The order of the components within the pixel is specified by
            /// PhotometricInterpretation. For example, for RGB data, the data is stored as
            /// RGBRGBRGB…
            /// </summary>
            Chunky = 1,
            /// <summary>
            /// The components are stored in separate “component planes.” The
            /// values in StripOffsets and StripByteCounts are then arranged as a 2-dimensional
            /// array, with SamplesPerPixel rows and StripsPerImage columns. (All of the columns
            /// for row 0 are stored first, followed by the columns of row 1, and so on.)
            /// PhotometricInterpretation describes the type of data stored in each component
            /// plane. For example, RGB data is stored with the Red components in one component
            /// plane, the Green in another, and the Blue in another.
            /// </summary>
            Planar = 2
        }

        public enum PhotometricInterpretation : ushort
        {
            WhiteIsZero = 0,
            BlackIsZero = 1,
            RGB = 2,
            PaletteColor = 3,
            TransparencyMask = 4,
            Separated = 5
        }
    }
}
