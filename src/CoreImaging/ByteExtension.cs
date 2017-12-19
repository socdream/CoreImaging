using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreImaging
{
    public static class ByteExtension
    {
        public static byte[] AdjustRange(this byte[] data, byte min = 0, byte max = 255)
        {
            var result = new byte[data.Length];

            var dataMin = data.Min();
            var dataMax = data.Max();
            var dataRange = dataMax - dataMin;
            var range = max - min;

            for (int i = 0; i < data.Length; i++)
                result[i] = (byte)(((double)(data[i] - dataMin) / dataRange) * range + min);

            return result;
        }
    }
}
