using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreImaging.PNG
{
    public class PngImage : Image
    {
        public PngImage(string path)
        {
            Data = Decode(path);
        }

        public PngImage(Image image)
        {
            DataStructure = image.DataStructure;
            Width = image.Width;
            Height = image.Height;
            Transforms = image.Transforms;
            Data = image.Data;
        }

        public bool Save(string path)
        {
            try
            {
                using (var stream = new System.IO.FileStream(path, System.IO.FileMode.Create))
                {
                    var header = new PngHeader();

                    stream.Write(header.Data, 0, header.Data.Length);

                    PngChunk.Write(stream, "IHDR", PngChunk.CreateIHDRData(new PngChunk.IHDRData()
                    {
                        Width = TransformedWidth,
                        Height = TransformedHeight,
                        BitDepth = 8,
                        ColorType = PngChunk.ColorTypeCode.ColorUsedAlphaChannelUsed,
                        CompressionMethod = PngChunk.CompressionMethodCode.Deflate,
                        FilterMethod = PngChunk.FilterTypeCode.None,
                        InterlaceMethod = PngChunk.InterlaceMethodCode.None
                    }));

                    DataStructure = ImageDataStructure.Rgba;

                    var imageData = new byte[Data.Length + TransformedHeight];

                    for (int i = 0; i < TransformedHeight; i++)
                    {
                        imageData[i * TransformedWidth * 4 + i] = (byte)PngChunk.FilterTypeCode.None;
                        Array.Copy(Data, i * TransformedWidth * 4, imageData, i * TransformedWidth * 4 + i + 1, TransformedWidth * 4);
                    }

                    using (var idatStream = new MemoryStream())
                    {
                        idatStream.WriteByte(0x78);
                        idatStream.WriteByte(0x9C);

                        using (var deflate = new System.IO.Compression.DeflateStream(idatStream, System.IO.Compression.CompressionMode.Compress))
                        {
                            var window = 32768;
                            var count = 0;

                            while (count * window < imageData.Length)
                            {
                                var buffer = imageData.Skip(count * window).Take(window).ToArray();
                                deflate.Write(buffer, 0, buffer.Length);
                                count++;
                            }

                            deflate.Dispose();
                            PngChunk.Write(stream, "IDAT", idatStream.ToArray());
                        }
                    }

                    PngChunk.Write(stream, "IEND", new byte[0]);
                }

            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private byte[] Decode(string path)
        {
            var result = new byte[0];

            using(var stream = new System.IO.FileStream(path, System.IO.FileMode.Open))
            {
                var header = new PngHeader(stream);
                
                var chunks = new List<PngChunk>();

                while(stream.Position < stream.Length)
                {
                    var chunk = new PngChunk(stream);

                    chunks.Add(chunk);

                    if (chunk.Type == "IEND")
                        break;
                }

                var ihdr = chunks.Where(a => a.Type == "IHDR").FirstOrDefault();
                var ihdrData = ihdr.IRDRInfo;

                Width = ihdrData.Width;
                Height = ihdrData.Height;

                var data = new List<byte>();

                for (var i = 0; i < chunks.Count; i++)
                    if (chunks[i].Type == "IDAT")
                        data.AddRange(chunks[i].Data);

                var pngData = new List<byte>();

                // The first byte 0x78 is called CMF, and the value means CM = 8, CINFO = 7. CM = 8 denotes the "deflate" 
                // compression method with a window size up to 32K. This is the method used by gzip and PNG. CINFO = 7 indicates a 
                // 32K window size. 
                // The 0x9C is called FLG and the value means FLEVEL = 2, CHECK = 28. 
                // The information in FLEVEL is not needed for decompression; it is there to indicate if recompression might be worthwhile. 
                // CHECK is set to whatever value is necessary such that CMF*256 + FLG is a multiple of 31
                // we'll ignore the 2 bytees
                using (var deflate = new System.IO.Compression.DeflateStream(new System.IO.MemoryStream(data.Skip(2).Take(data.Count - 2).ToArray()), System.IO.Compression.CompressionMode.Decompress))
                {
                    var buffer = new byte[32768];

                    while (deflate.CanRead)
                    {
                        int count = deflate.Read(buffer, 0, buffer.Length);

                        pngData.AddRange(buffer.Take(count));

                        if (count < 32768)
                            break;
                    }
                }

                var bytesPerPixel = 3;
                _dataStructure = ImageDataStructure.Rgb;

                switch (ihdrData.ColorType)
                {
                    case PngChunk.ColorTypeCode.ColorUsedAlphaChannelUsed:
                        bytesPerPixel = 4;
                        _dataStructure = ImageDataStructure.Rgba;
                        break;
                }


                result = new byte[Width * Height * bytesPerPixel];
                var previousScanLine = new byte[Width * bytesPerPixel];
                var scanLine = new byte[Width * bytesPerPixel];
                var pngArray = pngData.ToArray();

                var dataLineWithFilter = Width * bytesPerPixel + 1;

                for (var i = 0; i < Height; i++)
                {
                    // first byte of each line specifies the filter
                    var filter = (PngChunk.FilterTypeCode)pngArray[dataLineWithFilter * i];
                    
                    // copy the data of the scan line
                    Array.Copy(pngArray, dataLineWithFilter * i + 1, scanLine, 0, scanLine.Length);

                    previousScanLine = Defilter(filter, scanLine, previousScanLine, bytesPerPixel);

                    Array.Copy(previousScanLine, 0, result, scanLine.Length * i, scanLine.Length);
                }
            }

            return result;
        }

        private byte[] Filter(PngChunk.FilterTypeCode filter, byte[] scanLine, byte[] previousScanLine, int bytesPerPixel)
        {
            var result = new byte[scanLine.Length];

            switch (filter)
            {
                case PngChunk.FilterTypeCode.None:
                    Array.Copy(scanLine, result, scanLine.Length);
                    break;
                case PngChunk.FilterTypeCode.Sub:
                    for (var i = 0; i < scanLine.Length; i++)
                        if (i < bytesPerPixel)
                            result[i] = scanLine[i];
                        else
                            result[i] = (byte)(scanLine[i] - result[i - bytesPerPixel]);
                    break;
                case PngChunk.FilterTypeCode.Up:
                    for (var i = 0; i < scanLine.Length; i++)
                        result[i] = (byte)(scanLine[i] - previousScanLine[i]);
                    break;
                case PngChunk.FilterTypeCode.Average:
                    for (var i = 0; i < scanLine.Length; i++)
                        if (i < bytesPerPixel)
                            result[i] = (byte)(scanLine[i] - (byte)Math.Floor((previousScanLine[i]) / 2.0));
                        else
                            result[i] = (byte)(scanLine[i] - (byte)Math.Floor((result[i - bytesPerPixel] + previousScanLine[i]) / 2.0));
                    break;
                case PngChunk.FilterTypeCode.Paeth:
                    for (var i = 0; i < scanLine.Length; i++)
                        if (i < bytesPerPixel)
                            result[i] = (byte)(scanLine[i] - PaethPredictor(0, previousScanLine[i], 0));
                        else
                            result[i] = (byte)(scanLine[i] - PaethPredictor(result[i - bytesPerPixel], previousScanLine[i], previousScanLine[i - bytesPerPixel]));
                    break;
                default:
                    throw new Exception("Unexpected filter type");
            }

            return result;
        }

        private byte[] Defilter(PngChunk.FilterTypeCode filter, byte[] scanLine, byte[] previousScanLine, int bytesPerPixel)
        {
            var result = new byte[scanLine.Length];

            switch (filter)
            {
                case PngChunk.FilterTypeCode.None:
                    Array.Copy(scanLine, result, scanLine.Length);
                    break;
                case PngChunk.FilterTypeCode.Sub:
                    for (var i = 0; i < scanLine.Length; i++)
                        if (i < bytesPerPixel)
                            result[i] = scanLine[i];
                        else
                            result[i] = (byte)(scanLine[i] + result[i - bytesPerPixel]);
                    break;
                case PngChunk.FilterTypeCode.Up:
                    for (var i = 0; i < scanLine.Length; i++)
                        result[i] = (byte)(scanLine[i] + previousScanLine[i]);
                    break;
                case PngChunk.FilterTypeCode.Average:
                    for (var i = 0; i < scanLine.Length; i++)
                        if (i < bytesPerPixel)
                            result[i] = (byte)(scanLine[i] + (byte)Math.Floor((previousScanLine[i]) / 2.0));
                        else
                            result[i] = (byte)(scanLine[i] + (byte)Math.Floor((result[i - bytesPerPixel] + previousScanLine[i]) / 2.0));
                    break;
                case PngChunk.FilterTypeCode.Paeth:
                    for (var i = 0; i < scanLine.Length; i++)
                        if (i < bytesPerPixel)
                            result[i] = (byte)(scanLine[i] + PaethPredictor(0, previousScanLine[i], 0));
                        else
                            result[i] = (byte)(scanLine[i] + PaethPredictor(result[i - bytesPerPixel], previousScanLine[i], previousScanLine[i - bytesPerPixel]));
                    break;
                default:
                    throw new Exception("Unexpected filter type");
            }

            return result;
        }

        /// <summary>
        /// Computes the Paeth algorithm given three values
        /// </summary>
        /// <param name="a">Left pixel</param>
        /// <param name="b">Upper pixel</param>
        /// <param name="c">Upper left pixel</param>
        /// <returns></returns>
        private static byte PaethPredictor(byte a, byte b, byte c)
        {
            // initial estimate
            var p = a + b - c;

            //distances to a, b and c
            var pa = Math.Abs(p - a);
            var pb = Math.Abs(p - b);
            var pc = Math.Abs(p - c);

            // return nearest of a,b,c
            if (pa <= pb && pa <= pc)
                return a;

            if (pb <= pc)
                return b;

            return c;
        }
    }
}
