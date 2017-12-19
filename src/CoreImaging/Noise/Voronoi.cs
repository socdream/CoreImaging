using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreImaging.Noise
{
    public class Voronoi
    {
        public static byte[] Generate(int width, int height, int blockSize, List<Tuple<byte, byte, byte>> colors, DistanceMethod method = DistanceMethod.Euclidean, int seed = -1)
        {
            var data = new byte[width * height * 3];

            var countWidth = width / blockSize;
            var countHeight = height / blockSize;

            // generate points
            var points = new List<Tuple<int, int>>();

            if (seed == -1)
                seed = (int)DateTime.Now.Ticks;
            var rng = new Random();

            for (int i = 0; i < countWidth; i++)
            {
                for (int j = 0; j < countHeight; j++)
                {
                    var x = i * blockSize + rng.Next(blockSize);
                    var y = j * blockSize + rng.Next(blockSize);

                    points.Add(new Tuple<int, int>(x, y));
                }
            }

            // paint pixels
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var color = GetColor(i, j, points, colors, method);

                    data[(j * width + i) * 3] = color.Item1;
                    data[(j * width + i) * 3 + 1] = color.Item2;
                    data[(j * width + i) * 3 + 2] = color.Item3;
                }
            }

            return data;
        }

        private static Tuple<byte, byte, byte> GetColor(int x, int y, List<Tuple<int, int>> points, List<Tuple<byte, byte, byte>> colors, DistanceMethod method = DistanceMethod.Euclidean)
        {
            var closestDistance = int.MaxValue;
            var selected = -1;
            int i = 0;

            for (i = 0; i < points.Count; i++)
            {
                var x2 = (points[i].Item1 - x) * (points[i].Item1 - x);
                var y2 = (points[i].Item2 - y) * (points[i].Item2 - y);
                var distance = x2 + y2;

                if(closestDistance > distance)
                {
                    closestDistance = distance;
                    selected = i;
                }
            }

            return colors[selected % colors.Count];
        }

        public enum DistanceMethod
        {
            Euclidean,
            Manhattan
        }
    }
}
