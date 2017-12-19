using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreImaging.Noise
{
    public class White
    {
        public static byte[] Generate(int width, int height, int seed = -1)
        {
            var noise = new byte[width * height];
            
            if (seed == -1)
                seed = (int)DateTime.Now.Ticks;

            var rng = new Random(seed);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    noise[j * width + i] = (byte)rng.Next(256);
                }
            }

            return noise;
        }

        public static double Smooth(double x, double y, int noiseWidth, int noiseHeight, byte[] data)
        {
            //get fractional part of x and y
            double fractX = x % 1;
            double fractY = y % 1;

            //wrap around
            int x1 = ((int)x + noiseWidth) % noiseWidth;
            int y1 = ((int)y + noiseHeight) % noiseHeight;

            //neighbor values
            int x2 = (x1 + noiseWidth - 1) % noiseWidth;
            int y2 = (y1 + noiseHeight - 1) % noiseHeight;

            //smooth the noise with bilinear interpolation
            double value = 0.0;

            value += fractX * fractY * ((double)data[y1 * noiseWidth + x1] / 255.0);
            value += (1 - fractX) * fractY * ((double)data[y1 * noiseWidth + x2] / 255.0);
            value += fractX * (1 - fractY) * ((double)data[y2 * noiseWidth + x1] / 255.0);
            value += (1 - fractX) * (1 - fractY) * ((double)data[y2 * noiseWidth + x2] / 255.0);

            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="size">the initial zoom factor, the bigger the smoother</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="data">Image data (1 byte per pixel)</param>
        /// <returns></returns>
        public static double Turbulence(double x, double y, double size, int width, int height, byte[] data)
        {
            double value = 0.0, initialSize = size;

            while (size >= 1)
            {
                value += Smooth(x / size, y / size, width, height, data) * size;
                size /= 2.0;
            }

            return 128.0 * value / initialSize;
        }
    }
}
