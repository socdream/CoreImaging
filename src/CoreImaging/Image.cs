using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreImaging
{
    public class Image
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] Data { get; set; }
        public List<Transform> Transforms { get; set; } = new List<Transform>();

        public int TransformedWidth
        {
            get
            {
                if (Width == Height)
                    return Width;

                var width = Width;

                foreach (var item in Transforms)
                    if (item == Transform.Rotate90 || item == Transform.Rotate90 || item == Transform.Transpose)
                        width = (width == Width) ? Height : Width;

                return width;
            }
        }
        public int TransformedHeight
        {
            get
            {
                if (Width == Height)
                    return Height;

                var height = Height;

                foreach (var item in Transforms)
                    if (item == Transform.Rotate90 || item == Transform.Rotate90 || item == Transform.Transpose)
                        height = (height == Height) ? Width : Height;

                return height;
            }
        }

        protected ImageDataStructure _dataStructure = ImageDataStructure.Rgb;
        public ImageDataStructure DataStructure
        {
            get
            {
                return _dataStructure;
            }
            set
            {
                if(Data == null)
                {
                    _dataStructure = value;
                    return;
                }

                Data = ChangeDataStructure(Data, _dataStructure, value);
                _dataStructure = value;
            }
        }
        
        public byte[] GetPixel(int row, int column)
        {
            var bytesPerPixel = 3;

            if (DataStructure == ImageDataStructure.Bgra || DataStructure == ImageDataStructure.Rgba)
                bytesPerPixel = 4;
            else if (DataStructure == ImageDataStructure.Byte)
                bytesPerPixel = 1;

            return Data.Skip((row * Width + column) * bytesPerPixel).Take(bytesPerPixel).ToArray();
        }

        public Image Clone()
        {
            return Crop(0, 0, Width, Height);
        }

        public Image Crop(int row, int col, int width, int height)
        {
            if (row == 0 && col == 0 && width == Width && Height == height)
                return new Image()
                {
                    DataStructure = DataStructure,
                    Width = Width,
                    Height = height,
                    Data = (byte[])Data.Clone()
                };

            var bytesPerPixel = 3;
            var data = new byte[width * height * bytesPerPixel];

            for (int i = row; i < row + height; i++)
            {
                Array.Copy(Data, (i * Width + col) * bytesPerPixel, data, (i - row) * width * bytesPerPixel, width * bytesPerPixel);
            }

            return new Image()
            {
                DataStructure = DataStructure,
                Height = height,
                Width = width,
                Data = data
            };
        }

        public void Paste(Image image, int row, int col)
        {
            var bytesPerPixel = 3;
            var maxHeight = (image.Height + row > Height) ? Height - row : image.Height;
            var maxWidth = (image.Width + col > Width) ? Width - col : image.Width;

            var startRow = (row < 0) ? -row : 0;
            var startCol = (col < 0) ? -col : 0;

            for (int i = startRow; i < maxHeight; i++)
            {
                for (int j = startCol; j < maxWidth; j++)
                {
                    var baseId = ((i + row) * Width + j + col) * bytesPerPixel;
                    var copyId = (i * image.Width + j) * bytesPerPixel;

                    Data[baseId] = image.Data[copyId];
                    Data[baseId + 1] = image.Data[copyId + 1];
                    Data[baseId + 2] = image.Data[copyId + 2];
                }
            }
        }

        private static byte[] ChangeDataStructure(byte[] data, ImageDataStructure currentStruct, ImageDataStructure newStruct)
        {
            if (currentStruct == newStruct)
                return (byte[])data.Clone();

            switch (currentStruct)
            {
                case ImageDataStructure.Bgr:
                    switch (newStruct)
                    {
                        case ImageDataStructure.Bgra:
                            return AddAlphaChannel(data);
                        case ImageDataStructure.Byte:
                            throw new NotImplementedException("Method not available.");
                        case ImageDataStructure.Cmyk:
                            return RgbToCmyk(RgbToBgr(data));
                            throw new NotImplementedException("Method not available.");
                        case ImageDataStructure.Rgb:
                            return RgbToBgr(data);
                        case ImageDataStructure.Rgba:
                            return AddAlphaChannel(RgbToBgr(data));
                    }
                    break;
                case ImageDataStructure.Rgb:
                    switch (newStruct)
                    {
                        case ImageDataStructure.Bgra:
                            return AddAlphaChannel(RgbToBgr(data));
                        case ImageDataStructure.Byte:
                            throw new NotImplementedException("Method not available.");
                        case ImageDataStructure.Cmyk:
                            return RgbToCmyk(data);
                        case ImageDataStructure.Bgr:
                            return RgbToBgr(data);
                        case ImageDataStructure.Rgba:
                            return AddAlphaChannel(data);
                    }
                    break;
                case ImageDataStructure.Bgra:
                    switch (newStruct)
                    {
                        case ImageDataStructure.Byte:
                            throw new NotImplementedException("Method not available.");
                        case ImageDataStructure.Cmyk:
                            return RgbToCmyk(RgbToBgr(RemoveAlphaChannel(data)));
                        case ImageDataStructure.Rgb:
                            return RgbToBgr(RemoveAlphaChannel(data));
                        case ImageDataStructure.Bgr:
                            return RemoveAlphaChannel(data);
                        case ImageDataStructure.Rgba:
                            return RgbaToBgra(data);
                    }
                    break;
                case ImageDataStructure.Rgba:
                    switch (newStruct)
                    {
                        case ImageDataStructure.Byte:
                            throw new NotImplementedException("Method not available.");
                        case ImageDataStructure.Cmyk:
                            return RgbToCmyk(RemoveAlphaChannel(data));
                        case ImageDataStructure.Rgb:
                            return RemoveAlphaChannel(data);
                        case ImageDataStructure.Bgr:
                            return RgbToBgr(RemoveAlphaChannel(data));
                        case ImageDataStructure.Bgra:
                            return RgbaToBgra(data);
                    }
                    break;

                case ImageDataStructure.Byte:
                    switch (newStruct)
                    {
                        case ImageDataStructure.Rgb:
                        case ImageDataStructure.Bgr:
                            return ByteToRgb(data);
                        case ImageDataStructure.Bgra:
                        case ImageDataStructure.Rgba:
                            return AddAlphaChannel(ByteToRgb(data));
                    }
                    break;
                case ImageDataStructure.Cmyk:
                    switch (newStruct)
                    {
                        case ImageDataStructure.Rgb:
                            return CmykToRgb(data);
                        case ImageDataStructure.Bgr:
                            return RgbToBgr(CmykToRgb(data));
                        case ImageDataStructure.Rgba:
                            return AddAlphaChannel(CmykToRgb(data));
                        case ImageDataStructure.Bgra:
                            return AddAlphaChannel(RgbToBgr(CmykToRgb(data)));
                        case ImageDataStructure.Byte:
                            break;
                        default:
                            break;
                    }
                    break;
            }

            throw new NotImplementedException("No conversion method found.");
        }

        public static byte[] ByteToRgb(byte[] data)
        {
            var result = new byte[data.Length * 3];

            for (var i = 0; i < data.Length; i++)
            {
                result[i * 3] = data[i];
                result[i * 3 + 1] = data[i];
                result[i * 3 + 2] = data[i];
            }

            return result;
        }

        public static byte[] AddAlphaChannel(byte[] data, byte alpha = 255)
        {
            if (data.Length % 3 != 0)
                throw new ArgumentException("Image format needs to be a 3 channel format (Rgb or Bgr)");

            var result = new byte[(data.Length / 3) * 4];

            for (var i = 0; i < result.Length / 4; i++)
            {
                result[i * 4] = data[i * 3];
                result[i * 4 + 1] = data[i * 3 + 1];
                result[i * 4 + 2] = data[i * 3 + 2];
                result[i * 4 + 3] = alpha;
            }

            return result;
        }

        public static byte[] RemoveAlphaChannel(byte[] data, byte alpha = 255)
        {
            if (data.Length % 4 != 0)
                throw new ArgumentException("Image format needs to be a 4 channel format (Rgba or Bgra)");

            var result = new byte[(data.Length / 4) * 3];

            for (var i = 0; i < result.Length / 3; i++)
            {
                result[i * 3] = data[i * 4];
                result[i * 3 + 1] = data[i * 4 + 1];
                result[i * 3 + 2] = data[i * 4 + 2];
            }

            return result;
        }

        public static byte[] RgbToBgr(byte[] data)
        {
            var result = new byte[data.Length];

            for (var i = 0; i < data.Length / 3; i++)
            {
                result[i * 3] = data[i * 3 + 2];
                result[i * 3 + 1] = data[i * 3 + 1];
                result[i * 3 + 2] = data[i * 3];
            }

            return result;
        }

        public static byte[] RgbaToBgra(byte[] data)
        {
            var result = new byte[data.Length];

            for (var i = 0; i < data.Length / 4; i++)
            {
                result[i * 4] = data[i * 4 + 2];
                result[i * 4 + 1] = data[i * 4 + 1];
                result[i * 4 + 2] = data[i * 4];
                result[i * 4 + 3] = data[i * 4 + 3];
            }

            return result;
        }

        public static byte[] RgbToCmyk(byte red, byte green, byte blue)
        {
            if (red + green + blue == 0)
                return new byte[] { 0, 0, 0, 255 };

            var black = (byte)(255 - Math.Max(red, Math.Max(green, blue)));
            var cyan = (255 - red - black) * 255 / (255 - black);
            var magenta = (255 - green - black) * 255 / (255 - black);
            var yellow = (255 - blue - black) * 255 / (255 - black);
            return new byte[] { (byte)cyan, (byte)magenta, (byte)yellow, black };
        }

        public static byte[] RgbToCmyk(byte[] data)
        {
            if (data.Length % 3 != 0)
                throw new ArgumentException("Conversion not possible, rgb values are not multiple of 3.");

            var result = new List<byte>();

            for (int i = 0; i < data.Length; i += 3)
                result.AddRange(RgbToCmyk(data[i], data[i + 1], data[i + 2]));

            return result.ToArray();
        }

        public static byte[] CmykToRgb(double cyan, double magenta, double yellow, double black)
        {
            byte red = Convert.ToByte((1 - Math.Min(1, cyan * (1 - black) + black)) * 255);
            byte green = Convert.ToByte((1 - Math.Min(1, magenta * (1 - black) + black)) * 255);
            byte blue = Convert.ToByte((1 - Math.Min(1, yellow * (1 - black) + black)) * 255);
            return new[] { red, green, blue };
        }

        public static byte[] CmykToRgb(byte cyan, byte magenta, byte yellow, byte black)
        {
            //return CmykToRgb(cyan / 255.0, magenta / 255.0, yellow / 255.0, black / 255.0);
            var red = (255 - cyan) * (255 - black) / 255;
            var green = (255 - magenta) * (255 - black) / 255;
            var blue = (255 - yellow) * (255 - black) / 255;

            return new byte[] { (byte)red, (byte)green, (byte)blue };
        }

        public static byte[] CmykToRgb(byte[] data)
        {
            if (data.Length % 4 != 0)
                throw new ArgumentException("Conversion not possible, cmyk values are not multiple of 4.");

            var result = new List<byte>();

            for (int i = 0; i < data.Length; i+=4)
                result.AddRange(CmykToRgb(data[i], data[i + 1], data[i + 2], data[i + 3]));

            return result.ToArray();
        }

        public enum ImageDataStructure
        {
            Rgb,
            Bgr,
            Rgba,
            Bgra,
            Cmyk,
            Byte
        }

        public enum Transform
        {
            HorizontalMirror,
            VerticalMirror,
            Rotate90,
            Rotate180,
            Rotate270,
            Transpose
        }
    }
}
