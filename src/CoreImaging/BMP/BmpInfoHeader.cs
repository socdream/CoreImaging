// <copyright file="BmpInfoHeader.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreImaging.BMP
{
    /// <summary>
    /// This block of bytes tells the application detailed information
    /// about the image, which will be used to display the image on
    /// the screen.
    /// <see href="https://en.wikipedia.org/wiki/BMP_file_format"/>
    /// </summary>
    public class BmpInfoHeader
    {
        /// <summary>
        /// Defines of the data structure in the bitmap file.
        /// </summary>
        public const int Size = 40;

        /// <summary>
        /// Gets or sets the number of bits per pixel, which is the color depth of the image.
        /// Typical values are 1, 4, 8, 16, 24 and 32.
        /// </summary>
        public short BitsPerPixel { get; set; }

        /// <summary>
        /// Gets or sets the bitmap width in pixels (signed integer).
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the bitmap height in pixels (signed integer).
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the number of color planes being used. Must be set to 1.
        /// </summary>
        public short Planes { get; set; }

        /// <summary>
        /// Gets or sets the compression method being used.
        /// See the next table for a list of possible values.
        /// </summary>
        public BmpCompression Compression { get; set; }

        /// <summary>
        /// Gets or sets the size of this header (40 bytes)
        /// </summary>
        public int HeaderSize { get; set; }

        /// <summary>
        /// Gets or sets the image size. This is the size of the raw bitmap data (see below),
        /// and should not be confused with the file size.
        /// </summary>
        public int ImageSize { get; set; }

        /// <summary>
        /// Gets or sets the horizontal resolution of the image.
        /// (pixel per meter, signed integer)
        /// </summary>
        public int XPelsPerMeter { get; set; }

        /// <summary>
        /// Gets or sets the vertical resolution of the image.
        /// (pixel per meter, signed integer)
        /// </summary>
        public int YPelsPerMeter { get; set; }

        /// <summary>
        /// Gets or sets the number of colors in the color palette,
        /// or 0 to default to 2^n.
        /// </summary>
        public int ClrUsed { get; set; }

        /// <summary>
        /// Gets or sets the number of important colors used,
        /// or 0 when every color is important{ get; set; } generally ignored.
        /// </summary>
        public int ClrImportant { get; set; }

        public BmpInfoHeader(Stream stream)
        {
            var buffer = new byte[40];

            stream.Read(buffer, 0, buffer.Length);

            HeaderSize = BitConverter.ToInt32(buffer, 0);
            Width = BitConverter.ToInt32(buffer, 4);
            Height = BitConverter.ToInt32(buffer, 8);
            Planes = BitConverter.ToInt16(buffer, 12);
            BitsPerPixel = BitConverter.ToInt16(buffer, 14);

            Compression = (BmpCompression)BitConverter.ToInt32(buffer, 16);

            ImageSize = BitConverter.ToInt32(buffer, 20);
            XPelsPerMeter = BitConverter.ToInt32(buffer, 24);
            YPelsPerMeter = BitConverter.ToInt32(buffer, 28);
            ClrUsed = BitConverter.ToInt32(buffer, 32);
            ClrImportant = BitConverter.ToInt32(buffer, 36);
        }

        public BmpInfoHeader(short bpp, int width, int height)
        {
            HeaderSize = Size;

            Width = width;
            Height = height;
            Planes = 1;
            BitsPerPixel = bpp;
            Compression = (bpp == 24) ? BmpCompression.RGB : BmpCompression.RLE8;
            ImageSize = 0;
            XPelsPerMeter = 0;
            YPelsPerMeter = 0;
            ClrUsed = 0;
            ClrImportant = 0;
        }

        /// <summary>
        /// Defines how the compression type of the image data
        /// in the bitmap file.
        /// </summary>
        public enum BmpCompression : int
        {
            /// <summary>
            /// Each image row has a multiple of four elements. If the
            /// row has less elements, zeros will be added at the right side.
            /// The format depends on the number of bits, stored in the info header.
            /// If the number of bits are one, four or eight each pixel data is
            /// a index to the palette. If the number of bits are sixteen,
            /// twenty-four or thirty-two each pixel contains a color.
            /// </summary>
            RGB = 0,

            /// <summary>
            /// Two bytes are one data record. If the first byte is not zero, the
            /// next two half bytes will be repeated as much as the value of the first byte.
            /// If the first byte is zero, the record has different meanings, depending
            /// on the second byte. If the second byte is zero, it is the end of the row,
            /// if it is one, it is the end of the image.
            /// Not supported at the moment.
            /// </summary>
            RLE8 = 1,

            /// <summary>
            /// Two bytes are one data record. If the first byte is not zero, the
            /// next byte will be repeated as much as the value of the first byte.
            /// If the first byte is zero, the record has different meanings, depending
            /// on the second byte. If the second byte is zero, it is the end of the row,
            /// if it is one, it is the end of the image.
            /// Not supported at the moment.
            /// </summary>
            RLE4 = 2,

            /// <summary>
            /// Each image row has a multiple of four elements. If the
            /// row has less elements, zeros will be added at the right side.
            /// Not supported at the moment.
            /// </summary>
            BitFields = 3,

            /// <summary>
            /// The bitmap contains a JPG image.
            /// Not supported at the moment.
            /// </summary>
            JPEG = 4,

            /// <summary>
            /// The bitmap contains a PNG image.
            /// Not supported at the moment.
            /// </summary>
            PNG = 5
        }
    }
}
