using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace CoreImaging.Test
{
    [TestClass]
    public class ImagingTest
    {
        [TestMethod]
        public void PngTest()
        {
            /*var png1 = new PNG.PngImage($@"C:\Temp\Font.png");
            png1.Save(@"C:\Temp\pngTest1.png");

            png1.DataStructure = Image.ImageDataStructure.Bgr;

            BMP.BmpImage.Export(png1.Data, png1.Width, png1.Height, @"C:\Temp\pngTest2.bmp");*/

            var path = Path.GetTempFileName();

            var img = new Image()
            {
                Width = 16,
                Height = 16,
                DataStructure = Image.ImageDataStructure.Byte,
                Data = Enumerable.Range(0, 256).Select(a => (byte)(a % 256)).ToArray()
            };

            var png = new PNG.PngImage(img);
            png.Save(path);

            var read = new PNG.PngImage(path);

            var pixel1 = png.GetPixel(10, 10);
            var pixel2 = read.GetPixel(10, 10);
            Assert.IsTrue(pixel1[0] == pixel2[0] && pixel1[1] == pixel2[1] && pixel1[2] == pixel2[2]);
        }

        [TestMethod]
        public void CmykTiffTest()
        {
            var test = new Tiff.TiffImage(@"C:\Temp\cmyk.tif")
            {
                DataStructure = Image.ImageDataStructure.Rgb
            };

            var png = new PNG.PngImage(test);
            png.Save(@"C:\Temp\cmykTif.png");
        }

        [TestMethod]
        public void TiffTest()
        {
            var test = new Tiff.TiffImage(@"C:\Temp\Cap01.tif");
            
            test.Save(@"C:\Temp\tifTif.tif");
            var test2 = new Tiff.TiffImage(@"C:\Temp\tifTif.tif");

            BMP.BmpImage.Export(Image.RgbToBgr(test2.Data), test2.Width, test2.Height, @"C:\Temp\TifBmp.bmp");

            var path = Path.GetTempFileName();

            var img = new Image()
            {
                Width = 16,
                Height = 16,
                DataStructure = Image.ImageDataStructure.Byte,
                Data = Enumerable.Range(0, 256).Select(a => (byte)(a % 256)).ToArray()
            };

            var tif = new Tiff.TiffImage(img);
            tif.Save(path);

            var read = new Tiff.TiffImage(path);

            var pixel1 = tif.GetPixel(10, 10);
            var pixel2 = read.GetPixel(10, 10);
            Assert.IsTrue(pixel1[0] == pixel2[0] && pixel1[1] == pixel2[1] && pixel1[2] == pixel2[2]);
        }

        [TestMethod]
        public void TransformsTest()
        {
            var img = new Image()
            {
                Width = 2,
                Height = 2,
                DataStructure = Image.ImageDataStructure.Byte,
                Data = new byte[] { 255, 0, 0, 0 }
            };


        }
    }
}
