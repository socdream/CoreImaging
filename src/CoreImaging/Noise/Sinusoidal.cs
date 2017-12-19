using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreImaging.Noise
{
    // http://lodev.org/cgtutor/randomnoise.html
    public class Sinusoidal
    {
        public static byte[] Generate(int width, int height, double xPeriod = 0, double yPeriod = 0)
        {
            var data = new byte[width * height];

            //xPeriod and yPeriod together define the angle of the lines
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var xy = i * xPeriod / width + j * yPeriod / height;
                    var sine = (byte)(Math.Abs(Math.Sin(xy * Math.PI)) * 255);

                    data[j * height + i] = sine;
                }
            }

            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width">Width of the resulting image</param>
        /// <param name="height">Height of the resulting image</param>
        /// <param name="xPeriod">Defines repetition of marble lines in x direction</param>
        /// <param name="yPeriod">Defines repetition of marble lines in y direction</param>
        /// <param name="turbPower">Makes twists, the greater the more twists(5.0 - 1.0), 0 ==> it becomes a normal sine pattern</param>
        /// <param name="turbSize">Initial size of the turbulence, lowering this increaseas the agressive effect (16.0), increasing it makes it more subtle (128.0)</param>
        /// <returns></returns>
        public static byte[] Marble(int width, int height, double xPeriod = 0, double yPeriod = 0, double turbPower = 5.0, double turbSize = 32.0, int seed = -1)
        {
            var noise = White.Generate(width, height, seed);
            var data = new byte[width * height];

            //xPeriod and yPeriod together define the angle of the lines
            //xPeriod and yPeriod both 0 ==> it becomes a normal clouds or turbulence pattern
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var xy = i * xPeriod / width + j * yPeriod / height + turbPower * White.Turbulence(i, j, turbSize, width, height, noise) / 256.0;
                    var sine = (byte)(Math.Abs(Math.Sin(xy * Math.PI)) * 255);

                    data[j * width + i] = sine;
                }
            }

            return data;
        }

        public static byte[] WoodRings(int width, int height, double xyPeriod = 12.0, double turbPower = 0.1, double turbSize = 32.0, int seed = -1)
        {
            var noise = White.Generate(width, height, seed);
            var data = new byte[width * height];

            //xPeriod and yPeriod together define the angle of the lines
            //xPeriod and yPeriod both 0 ==> it becomes a normal clouds or turbulence pattern
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var xValue = (i - width / 2.0) / width;
                    var yValue = (j - height / 2.0) / height;
                    var distance = Math.Sqrt(xValue * xValue + yValue * yValue) + turbPower * White.Turbulence(i, j, turbSize, width, height, noise) / 256.0;
                     
                    var sine = (byte)(Math.Abs(Math.Sin(2 * xyPeriod * distance * Math.PI)) * 128.0);

                    data[j * width + i] = sine;
                }
            }

            return data;
        }
    }
}
