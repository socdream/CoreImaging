using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreImaging.JPEG
{
    public class JpegEncoder
    {
        public static byte[] RgbToYCbCr(byte[] data)
        {
            return Enumerable.Range(0, data.Length / 3).SelectMany(i =>
            {
                var y = 0.299 * data[i * 3] + 0.587 * data[i * 3 + 1] + 0.114 * data[i * 3 + 2];
                var cb = 0.564 * (data[i * 3 + 2] - y);
                var cr = 0.713 * (data[i * 3] - y);

                return new byte[] { (byte)y, (byte)cb, (byte)cr };
            }).ToArray();
        }
        public static byte[] YCbCrToRgb(byte[] data)
        {
            return Enumerable.Range(0, data.Length / 3).SelectMany(i =>
            {
                var r = data[i * 3] + 1.402 * data[i * 3 + 2];
                var g = data[i * 3] + 0.344 * data[i * 3 + 1] - 0.714 * data[i * 3 + 2];
                var b = data[i * 3] + 1.772 * data[i * 3 + 1];

                return new byte[] { (byte)r, (byte)g, (byte)b };
            }).ToArray();
        }
    }
}
