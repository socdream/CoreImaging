using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CoreImaging.JPEG
{
    public class JpegImage : Image
    {
        public List<JpegSegment> Segments { get; set; } = new List<JpegSegment>();

        public JpegImage(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new BinaryReader(stream))
            {
                var header = new JpegHeader(reader);

                while (Segments.FirstOrDefault(a => a is EoiSegment) != null)
                    Segments.Add(JpegSegment.ReadSegment(reader));
            }
        }
        
        public enum Marker : byte
        {
            SOI = 0xd8, // Start of Image
            APP0 = 0xe0, // JFIF application segment
            APP1 = 0xe1, // Other APP segments
            APP2 = 0xe2, // Other APP segments
            APP3 = 0xe3, // Other APP segments
            APP4 = 0xe4, // Other APP segments
            APP5 = 0xe5, // Other APP segments
            APP6 = 0xe6, // Other APP segments
            APP7 = 0xe7, // Other APP segments
            APP8 = 0xe8, // Other APP segments
            APP9 = 0xe9, // Other APP segments
            APPA = 0xea, // Other APP segments
            APPB = 0xeb, // Other APP segments
            APPC = 0xec, // Other APP segments
            APPD = 0xed, // Other APP segments
            APPE = 0xee, // Other APP segments
            APPF = 0xef, // Other APP segments
            SOF0 = 0xc0, // Start Of Frame (baseline JPEG)
            SOF1 = 0xc1, // Start Of Frame(baseline JPEG)
            SOF2 = 0xc2, // usually unsupported
            SOF3 = 0xc3, // usually unsupported
            SOF5 = 0xc5, // usually unsupported
            SOF6 = 0xc6, // usually unsupported
            SOF7 = 0xc7, // usually unsupported
            SOF9 = 0xc9, // for arithmetic coding, usually unsupported
            SOF10 = 0xca, // usually unsupported
            SOF11 = 0xcb, // usually unsupported
            SOF13 = 0xcd, // usually unsupported
            SOF14 = 0xce, // usually unsupported
            SOF15 = 0xcf, // usually unsupported
            DHT = 0xc4, // Huffman Table
            DQT = 0xdb, // Quantization Table
            SOS = 0xda, // Start of Scan
            EOI = 0xd9, // End of Image
        }
    }
}
