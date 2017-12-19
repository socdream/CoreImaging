using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreImaging.BMP
{
    public class BmpImage : Image
    {
        public BmpFileHeader FileHeader { get; set; }
        public BmpInfoHeader InfoHeader { get; set; }

        public BmpImage(string path)
        {
            using (var stream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                FileHeader = new BmpFileHeader(stream);
                InfoHeader = new BmpInfoHeader(stream);

                DataStructure = ImageDataStructure.Bgr;
                Width = InfoHeader.Width;
                Height = InfoHeader.Height;

                var bytesPerPixel = 3;
                Data = new byte[Width * Height * bytesPerPixel];

                var padding = (Width * bytesPerPixel) % 4;

                for (int row = 0; row < Height; row++)
                {
                    stream.Read(Data, row * Width * bytesPerPixel, Width * bytesPerPixel);
                    
                    if (padding != 0)
                        padding = 4 - padding;

                    stream.Position += padding;
                }
            }
        }

        /// <summary>
        /// Write 24 bit colors byte array in BGR format
        /// </summary>
        /// <param name="data"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="path"></param>
        /// <param name="reverseScanLines">Most image formats use scan lines from top to bottom, bmp stores it bottom to top</param>
        public static void Export(byte[] data, int width, int height, string path, bool reverseScanLines = true)
        {
            // Determine the size of the pixel data given the bit depth, width, and
            // height of the bitmap.  Note: Bitmap pixel data is always aligned to 4 byte
            // boundaries.
            var bytesPerPixel = data.Length / (width * height); //3;
            var bpp = (short)(8 * bytesPerPixel);
            var extraBytes = (width * bytesPerPixel) % 4;
            var adjustedLineSize = bytesPerPixel * (width + extraBytes);
            //var bytesPerLine = 4 * (int)Math.Floor((Width * bpp + 31) / 32.0);

            BmpInfoHeader infoHeader = new BmpInfoHeader(bpp, width, height)
            {
                ImageSize = height * adjustedLineSize
            };

            BmpFileHeader fileHeader = new BmpFileHeader
            {
                Offset = 54,
                FileSize = 54 + infoHeader.ImageSize
            };

            using (var stream = new System.IO.FileStream(path, System.IO.FileMode.Create))
            {
                //write bmp file header
                WriteHeader(stream, fileHeader);
                WriteInfo(stream, infoHeader);
                WriteImage(stream, data, width, height, bytesPerPixel, reverseScanLines);
            }
        }

        /// <summary>
        /// Writes the bitmap header data to the binary stream.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="EndianBinaryWriter"/> containing the stream to write to.
        /// </param>
        /// <param name="fileHeader">
        /// The <see cref="BmpFileHeader"/> containing the header data.
        /// </param>
        private static void WriteHeader(System.IO.Stream stream, BmpFileHeader fileHeader)
        {
            stream.Write(BitConverter.GetBytes(fileHeader.Type), 0, 2);
            stream.Write(BitConverter.GetBytes(fileHeader.FileSize), 0, 4);
            stream.Write(BitConverter.GetBytes(fileHeader.Reserved), 0, 4);
            stream.Write(BitConverter.GetBytes(fileHeader.Offset), 0, 4);
        }

        /// <summary>
        /// Writes the bitmap information to the binary stream.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="EndianBinaryWriter"/> containing the stream to write to.
        /// </param>
        /// <param name="infoHeader">
        /// The <see cref="BmpFileHeader"/> containing the detailed information about the image.
        /// </param>
        private static void WriteInfo(System.IO.Stream writer, BmpInfoHeader infoHeader)
        {
            writer.Write(BitConverter.GetBytes(infoHeader.HeaderSize), 0, 4);
            writer.Write(BitConverter.GetBytes(infoHeader.Width), 0, 4);
            writer.Write(BitConverter.GetBytes(infoHeader.Height), 0, 4);
            writer.Write(BitConverter.GetBytes(infoHeader.Planes), 0, 2);
            writer.Write(BitConverter.GetBytes(infoHeader.BitsPerPixel), 0, 2);

            writer.Write(BitConverter.GetBytes((int)infoHeader.Compression), 0, 4);

            writer.Write(BitConverter.GetBytes(infoHeader.ImageSize), 0, 4);
            writer.Write(BitConverter.GetBytes(infoHeader.XPelsPerMeter), 0, 4);
            writer.Write(BitConverter.GetBytes(infoHeader.YPelsPerMeter), 0, 4);
            writer.Write(BitConverter.GetBytes(infoHeader.ClrUsed), 0, 4);
            writer.Write(BitConverter.GetBytes(infoHeader.ClrImportant), 0, 4);
        }

        private static void WriteImage(System.IO.Stream writer, byte[] data, int width, int height, int bytesPerPixel, bool reverseScanLines = false)
        {
            var padding = (width * bytesPerPixel) % 4;

            if (padding != 0)
                padding = 4 - padding;

            var paddingBuf = new byte[padding];

            if (reverseScanLines)
                for (var i = height - 1; i >= 0; i--)
                    WriteScanLine(writer, data.Skip(i * width * bytesPerPixel).Take(width * bytesPerPixel).ToArray(), paddingBuf);
            else
                for (var i = 0; i < height; i++)
                    WriteScanLine(writer, data.Skip(i * width * bytesPerPixel).Take(width * bytesPerPixel).ToArray(), paddingBuf);
        }

        private static void WriteScanLine(System.IO.Stream stream, byte[] data, byte[] paddingBuf)
        {
            stream.Write(data, 0, data.Length);

            if (paddingBuf != null && paddingBuf.Length != 0)
                stream.Write(paddingBuf, 0, paddingBuf.Length);
        }
    }
}
