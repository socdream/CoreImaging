using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreImaging.Test
{
    [TestClass]
    public class NoiseTest
    {
        [TestMethod]
        public void PerlinTest()
        {
            var width = 512;
            var height = 512;
            var blockSize = 32;

            var data = new byte[width * height];
            var data2 = new byte[width * height];
            var perlin = new CoreImaging.Noise.Perlin();

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    data[i * width + j] = (byte)(perlin.Generate((double)i / blockSize, (double)j / blockSize, 0) * 256.0);
                    data2[i * width + j] = (byte)(perlin.OctavePerlin((double)i / blockSize, (double)j / blockSize, 0, 10, 1) * 256.0);
                }
            }

            var perlinOctave = Image.ByteToRgb(data2);

            BMP.BmpImage.Export(Image.ByteToRgb(data), width, height, @"C:\Temp\perlin.bmp");
            BMP.BmpImage.Export(perlinOctave, width, height, @"C:\Temp\perlinOctave.bmp");
        }

        [TestMethod]
        public void VoronoiTest()
        {
            var width = 512;
            var height = 512;

            var data3 = Noise.Voronoi.Generate(width, height, (height > width) ? width / 3 : height / 3, new List<Tuple<byte, byte, byte>>()
            {
                new Tuple<byte, byte, byte>(209, 169, 133),
                new Tuple<byte, byte, byte>(165, 131, 94),
                new Tuple<byte, byte, byte>(206, 174, 133)
            }, Noise.Voronoi.DistanceMethod.Euclidean);

            data3 = Image.RgbToBgr(data3);

            BMP.BmpImage.Export(data3, width, height, @"C:\Temp\voronoi.bmp");
        }

        [TestMethod]
        public void WhiteNoiseTest()
        {
            var width = 512;
            var height = 512;

            var noise = Noise.White.Generate(width, height);

            CoreImaging.BMP.BmpImage.Export(Image.ByteToRgb(noise), width, height, @"C:\Temp\Marble\01 noise.bmp");

            
        }

        [TestMethod]
        public void SmoothNoiseTest()
        {
            var width = 512;
            var height = 512;
            
            var noise = Noise.White.Generate(width, height);

            var smoothed = new byte[width * height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    smoothed[y * width + x] = (byte)(Noise.White.Smooth(x / 8.0, y / 8.0, width, height, noise) * 255);
                }
            }
        }

        [TestMethod]
        public void TurbulenceTest()
        {
            var width = 512;
            var height = 512;

            var noise = Noise.White.Generate(width, height);

            var turbulence = new byte[width * height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    turbulence[y * width + x] = (byte)(Noise.White.Turbulence(x, y, 64.0, width, height, noise));
                }
            }

            BMP.BmpImage.Export(Image.ByteToRgb(turbulence), width, height, @"C:\Temp\Marble\03 turbulence.bmp");
        }

        [TestMethod]
        public void SineTest()
        {
            var width = 512;
            var height = 512;

            var sine = CoreImaging.Noise.Sinusoidal.Generate(width, height, 5.0, 10.0);

            BMP.BmpImage.Export(CoreImaging.Image.ByteToRgb(sine), width, height, @"C:\Temp\Marble\04 sine.bmp");
        }

        [TestMethod]
        public void MarbleTest()
        {
            var width = 512;
            var height = 512;

            var marble = Noise.Sinusoidal.Marble(width, height, 5.0, 10.0, 5.0, 128.0);

            BMP.BmpImage.Export(Image.ByteToRgb(marble), width, height, @"C:\Temp\Marble\05 marble.bmp");
        }

        [TestMethod]
        public void WoodRingsTest()
        {
            var width = 512;
            var height = 512;

            var wood = Noise.Sinusoidal.WoodRings(width, height, 12.0, 0.05, 32.0);

            BMP.BmpImage.Export(Image.ByteToRgb(wood), width, height, @"C:\Temp\Marble\06 wood.bmp");

            var woodRanged = wood.AdjustRange();

            BMP.BmpImage.Export(Image.ByteToRgb(woodRanged), width, height, @"C:\Temp\Marble\07 wood ranged.bmp");
        }

        [TestMethod]
        public void MandlebrotFractalTest()
        {
            var width = 512;
            var height = 512;

            // set 1: -1.3, -1.5, 0.1, -0.1
            // set 2: -1.38, -1.42, 0.02, -0.02
            var mandel = Fractal.MandelbrotSet(width, height, -1.38, -1.42, 0.02, -0.02);

            BMP.BmpImage.Export(mandel, width, height, @"C:\Temp\Marble\08 mandelbrot.bmp");
        }

        [TestMethod]
        public void JuliaFractalTest()
        {
            var width = 512;
            var height = 512;

            var julia = Fractal.Julia(width, height);

            BMP.BmpImage.Export(julia, width, height, @"C:\Temp\Marble\09 julia.bmp");
        }
    }
}
