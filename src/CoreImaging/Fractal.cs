using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace CoreImaging
{
    public class Fractal
    {
        public static List<Tuple<byte, byte, byte>> GenerateColorPalette()
        {
            List<Tuple<byte, byte, byte>> retVal = new List<Tuple<byte, byte, byte>>();
            for (int i = 0; i <= 255; i++)
            {
                retVal.Add(new Tuple<byte, byte, byte>(255, (byte)i, (byte)i));
            }
            return retVal;
        }

        public static byte[] MandelbrotSet2(int width, int height, double rMax = 1, double rMin = -2, double iMax = 1, double iMin = -1)
        {
            List<Tuple<byte, byte, byte>> Palette = GenerateColorPalette();
            var img = new byte[width * height * 3]; // Bitmap to contain the set

            double rScale = (Math.Abs(rMin) + Math.Abs(rMax)) / width; // Amount to move each pixel in the real numbers
            double iScale = (Math.Abs(iMin) + Math.Abs(iMax)) / height; // Amount to move each pixel in the imaginary numbers

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var c = new Complex(x * rScale + rMin, y * iScale + iMin); // Scaled complex number
                    Complex z = c;
                    for (int i = 0; i < Palette.Count; i++) // 255 iterations with the method we already wrote
                    {
                        if (z.Magnitude >= 2.0)
                        {
                            img[(y * width + x) * 3] = Palette[i].Item1;
                            img[(y * width + x) * 3 + 1] = Palette[i].Item2;
                            img[(y * width + x) * 3 + 2] = Palette[i].Item3;
                            break; // We're done with this loop
                        }
                        else
                        {
                            z = c + Complex.Pow(z, 2); // Z = Zlast^2 + C
                        }
                    }
                }
            }

            return img;
        }

        public static byte[] MandelbrotSet(int width, int height, double Xmax = 2, double Xmin = -2, double Ymax = 2, double Ymin = -2)
        {
            var pic = new byte[width * height * 3];

            double zx = 0;
            double zy = 0;
            double cx = 0;
            double cy = 0;
            double xzoom = ((Xmax - Xmin) / Convert.ToDouble(width));
            double yzoom = ((Ymax - Ymin) / Convert.ToDouble(height));
            double tempzx = 0;

            int loopgo = 0;
            for (int x = 0; x < width; x++)
            {
                cx = (xzoom * x) - Math.Abs(Xmin);

                for (int y = 0; y < height; y++)
                {
                    zx = 0;
                    zy = 0;
                    cy = (yzoom * y) - Math.Abs(Ymin);
                    loopgo = 0;

                    while (zx * zx + zy * zy <= 4 && loopgo < 1000)
                    {
                        loopgo++;
                        tempzx = zx;
                        zx = (zx * zx) - (zy * zy) + cx;
                        zy = (2 * tempzx * zy) + cy;
                    }

                    if (loopgo != 1000)
                    {
                        pic[(y * width + x) * 3] = (byte)(loopgo % 128 * 2);
                        pic[(y * width + x) * 3 + 1] = (byte)(loopgo % 32 * 7);
                        pic[(y * width + x) * 3 + 2] = (byte)(loopgo % 16 * 14);
                    }
                }
            }

            return pic;

        }

        static public Tuple<byte, byte, byte> InterpolateColors(int s1, int s2, int weigth)
        {
            var c1 = new Tuple<byte, byte, byte>((byte)((s1 >> 16) & 0xFF), (byte)((s1 >> 8) & 0xFF), (byte)(s1 & 0xFF));
            var c2 = new Tuple<byte, byte, byte>((byte)((s2 >> 16) & 0xFF), (byte)((s2 >> 8) & 0xFF), (byte)(s2 & 0xFF));

            byte red = (byte)(((int)c1.Item1 + ((int)((c2.Item1 - c1.Item1) * weigth) >> 8)) & 0xff);
            byte green = (byte)(((int)c1.Item2 + ((int)((c2.Item2 - c1.Item2) * weigth) >> 8)) & 0xff);
            byte blue = (byte)(((int)c1.Item3 + ((int)((c2.Item3 - c1.Item1) * weigth) >> 8)) & 0xff);

            return new Tuple<byte, byte, byte>(red, green, blue);
        }

        public static byte[] Julia(int width, int height, int iterations = 128, bool renderInterpolated = false, double W = 4.0,
            double H = 4.0,
            double X = 0.5,
            double Y = -0.2,
            double XC = 0.0,
            double YC = 0.0)
        {
            iterations -= 1;
            var Palette = new int[] {
                            -16777108, -16777099, -16777089, -16777079, -16777069, -16777059,
                            -16777050, -16777040, -16777030, -16777020, -16777010, -16777001, 
                            -16776991, -16776981, -16776971, -16776961, -15790081, -14737409, 
                            -13684737, -12632065, -11579393, -10526721, -9474049, -8421377, 
                            -7368705, -6316033, -5263361, -4210689, -3158017, -2105345, -1052673, 
                            -1, -10, -18, -26, -34, -43, -65843, -59, -67, -76, -84, -92, -100,
                            -65900, -65909, -125, -133, -141, -150, -158, -166, -174, -182, -191, 
                            -199, -207, -215, -224, -232, -240, -248, -256, -16777216, -12632257,
                            -12566464, -12500671, -12434878, -12369085, -12303292, -12237499,
                            -12171706, -12105913, -12040120, -11974327, -11908534, -11842741, 
                            -11776948, -11711155, -11645362, -11579569, -11513776, -11447983, 
                            -11382190, -11316397, -11250604, -11184811, -11119018, -11053225, 
                            -10987432, -10921639, -10855846, -10790053, -10724260, -10658467, 
                            -10592674, -10526881, -10461088, -10395295, -10329502, -10263709, 
                            -10197916, -10132123, -10066330, -10000537, -9934744, -9868951, -9803158, 
                            -9737365, -9671572, -9605779, -9539986, -9474193, -9408400, -9342607, 
                            -9276814, -9211021, -9145228, -9079435, -9013642, -8947849, -8882056, 
                            -8816263, -8750470, -8684677, -8618884, -8553091, -16777216, -8421505,
                            -8355712, -8289919, -8224126, -8158333, -8092540, -8026747, -7960954, 
                            -7895161, -7829368, -7763575, -7697782, -7631989, -7566196, -7500403,
                            -7434610, -7368817, -7303024, -7237231, -7171438, -7105645, -7039852,
                            -6974059, -6908266, -6842473, -6776680, -6710887, -6645094, -6579301, 
                            -6513508, -6447715, -6381922, -6316129, -6250336, -6184543, -6118750, 
                            -6052957, -5987164, -5921371, -5855578, -5789785, -5723992, -5658199, 
                            -5592406, -5526613, -5460820, -5395027, -5329234, -5263441, -5197648,
                            -5131855, -5066062, -5000269, -4934476, -4868683, -4802890, -4737097,
                            -4671304, -4605511, -4539718, -4473925, -4408132, -4342339, -4276546, 
                            -4210753, -4144960, -4079167, -4013374, -3947581, -3881788, -3815995, 
                            -3750202, -3684409, -3618616, -3552823, -3487030, -3421237, -3355444,
                            -3289651, -3223858, -3158065, -3092272, -3026479, -2960686, -2894893,
                            -2829100, -2763307, -2697514, -2631721, -2565928, -2500135, -2434342,
                            -2368549, -2302756, -2236963, -2171170, -2105377, -2039584, -1973791, 
                            -1907998, -1842205, -1776412, -1710619, -1644826, -1579033, -1513240, 
                            -1447447, -1381654, -1315861, -1250068, -1184275, -1118482, -1052689, 
                            -986896, -921103, -855310, -789517, -723724, -657931, -592138, -526345, 
                            -460552, -394759, -328966, -263173, -197380, -131587, -16777216 };
            
            double r1 = 0;
            double i1 = 0;
            double r1pow2 = 0;
            double i1pow2 = 0;
            double rpow = 0;
            double rlastpow = 0;
            int iter = 0;
            int idx = 0;
            double xs = (X - W / 2.0);
            double ys = (Y - H / 2.0);
            double xd = W / (double)width;
            double yd = H / (double)height;
            double y1 = ys + yd;

            var result = new byte[width * height * 3];

            for (int y = 0; y < height; y++)
            {
                idx = y * width;
                double x1 = xs;

                for (int x = 0; x < width; x++)
                {
                    iter = 0;
                    r1 = x1;
                    i1 = y1;
                    r1pow2 = x1 * x1;
                    i1pow2 = y1 * y1;

                    rpow = 0;
                    rlastpow = 0;
                    while ((iter < iterations) && (rpow < 4))
                    {
                        r1pow2 = r1 * r1;
                        i1pow2 = i1 * i1;
                        i1 = 2 * i1 * r1 + YC;
                        r1 = r1pow2 - i1pow2 + XC;
                        rlastpow = rpow;
                        rpow = r1pow2 + i1pow2;
                        iter++;
                    }

                    if (renderInterpolated)
                    {
                        double count_f = iter + (4 - rlastpow) / (rpow - rlastpow) - 1;
                        int factor = (int)((1.0 - (iter - count_f)) * 255);
                        var color = InterpolateColors(Palette[(iter - 1) % Palette.Length], Palette[iter % Palette.Length], factor);

                        result[(y * width + x) * 3] = color.Item1;
                        result[(y * width + x) * 3 + 1] = color.Item2;
                        result[(y * width + x) * 3 + 2] = color.Item3;
                    }
                    else
                    {
                        iter = iter % Palette.Length;

                        result[(y * width + x) * 3] = (byte)((Palette[iter] >> 16) & 0xFF);
                        result[(y * width + x) * 3 + 1] = (byte)((Palette[iter] >> 8) & 0xFF);
                        result[(y * width + x) * 3 + 2] = (byte)(Palette[iter] & 0xFF);
                    }

                    x1 += xd;
                }

                y1 += yd;
            }

            return result;
        }
    }
}
