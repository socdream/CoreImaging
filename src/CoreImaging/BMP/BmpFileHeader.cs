﻿// <copyright file="BmpFileHeader.cs" company="James Jackson-South">
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
    /// Stores general information about the Bitmap file.
    /// <see href="https://en.wikipedia.org/wiki/BMP_file_format" />
    /// </summary>
    /// <remarks>
    /// The first two bytes of the Bitmap file format
    /// (thus the Bitmap header) are stored in big-endian order.
    /// All of the other integer values are stored in little-endian format
    /// (i.e. least-significant byte first).
    /// </remarks>
    public class BmpFileHeader
    {
        /// <summary>
        /// Defines of the data structure in the bitmap file.
        /// </summary>
        public const int Size = 14;

        /// <summary>
        /// Gets or sets the Bitmap identifier.
        /// The field used to identify the bitmap file: 0x42 0x4D
        /// (Hex code points for B and M)
        /// </summary>
        public short Type { get; set; } = 19778;

        /// <summary>
        /// Gets or sets the size of the bitmap file in bytes.
        /// </summary>
        public int FileSize { get; set; }

        /// <summary>
        /// Gets or sets any reserved data; actual value depends on the application
        /// that creates the image.
        /// </summary>
        public int Reserved { get; set; }

        /// <summary>
        /// Gets or sets the offset, i.e. starting address, of the byte where
        /// the bitmap data can be found.
        /// </summary>
        public int Offset { get; set; }

        public BmpFileHeader() { }

        public BmpFileHeader(Stream stream)
        {
            var buffer = new byte[14];

            stream.Read(buffer, 0, buffer.Length);
            
            Type = BitConverter.ToInt16(buffer, 0);
            FileSize = BitConverter.ToInt32(buffer, 2);
            Offset = BitConverter.ToInt32(buffer, 10);
        }
    }
}
