namespace TrueCraft.Core.TerrainGen.Noise
{
    //Perlin Noise implemented using http://freespace.virgin.net/hugo.elias/models/m_perlin.htm
    public class Perlin : NoiseGen
    {
        public Perlin(int seed)
        {
            Seed = seed;
            Octaves = 2;
            Amplitude = 2;
            Persistance = 1;
            Frequency = 1;
            Lacunarity = 2;
            Interpolation = InterpolateType.COSINE;
        }

        public int Seed { get; set; }
        public int Octaves { get; set; }
        public double Amplitude { get; set; }
        public double Persistance { get; set; }
        public double Frequency { get; set; }
        public double Lacunarity { get; set; }
        public InterpolateType Interpolation { get; set; }

        /*
         * Psuedo-random number generator methods.
         * For this we use integer noise
         */
        private double Noise2D(double X, double Y)
        {
            var N = ((int) X * 1619 + (int) Y * 31337 * 1013 * Seed) & 0x7fffffff;
            N = (N << 13) ^ N;
            return 1.0 - ((N * (N * N * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0;
        }

        private double Noise3D(double X, double Y, double Z)
        {
            var N = ((int) X * 1619 + (int) Y * 31337 + (int) Z * 52591 * 1013 * Seed) & 0x7fffffff;
            N = (N << 13) ^ N;
            return 1.0 - ((N * (N * N * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0;
        }

        /*
         * Perlin Noise methods
         */
        public override double Value2D(double X, double Y)
        {
            var Total = 0.0;
            var _Frequency = Frequency;
            var _Amplitude = Amplitude;

            for (var I = 0; I < Octaves; I++)
            {
                Total += Interpolated2D(X * _Frequency, Y * _Frequency) * _Amplitude;
                _Frequency *= Lacunarity;
                _Amplitude *= Persistance;
            }

            return Total;
        }

        public override double Value3D(double X, double Y, double Z)
        {
            var Total = 0.0;
            var _Frequency = Frequency;
            var _Amplitude = Amplitude;

            for (var I = 0; I < Octaves; I++)
            {
                Total += Interpolated3D(X * _Frequency, Y * _Frequency, Z * _Frequency) * _Amplitude;
                _Frequency *= Lacunarity;
                _Amplitude *= Persistance;
            }

            return Total;
        }

        /*
         * Smooth Noise Methods
         */
        private double Smooth2D(double X, double Y)
        {
            var X0 = X - 1;
            var X1 = X + 1;
            var Y0 = Y - 1;
            var Y1 = Y + 1;

            var Corners = (Noise2D(X0, Y0) + Noise2D(X1, Y0) + Noise2D(X0, Y1) + Noise2D(X1, Y1)) / 16;
            var Sides = (Noise2D(X0, Y) + Noise2D(X1, Y) + Noise2D(X, Y0) + Noise2D(X, Y1)) / 8;
            var Center = Noise2D(X, Y) / 4;

            return Corners + Sides + Center;
        }

        private double Smooth3D(double X, double Y, double Z)
        {
            double edges = 0;
            edges += Noise3D(X + 1, Y + 1, Z) + Noise3D(X - 1, Y + 1, Z) + Noise3D(X, Y + 1, Z + 1) +
                     Noise3D(X, Y + 1, Z - 1);
            edges += Noise3D(X + 1, Y - 1, Z) + Noise3D(X - 1, Y - 1, Z) + Noise3D(X, Y - 1, Z + 1) +
                     Noise3D(X, Y - 1, Z - 1);
            edges += Noise3D(X + 1, Y, Z + 1) + Noise3D(X + 1, Y, Z - 1) + Noise3D(X - 1, Y, Z + 1) +
                     Noise3D(X - 1, Y, Z - 1);
            edges /= 48;
            double corners = 0;
            corners += Noise3D(X - 1, Y - 1, Z - 1) + Noise3D(X - 1, Y - 1, Z + 1) + Noise3D(X - 1, Y + 1, Z - 1) +
                       Noise3D(X - 1, Y + 1, Z + 1);
            corners += Noise3D(X + 1, Y - 1, Z - 1) + Noise3D(X + 1, Y - 1, Z + 1) + Noise3D(X + 1, Y + 1, Z - 1) +
                       Noise3D(X + 1, Y + 1, Z + 1);
            corners /= 32;
            double sides = 0;
            corners += Noise3D(X - 1, Y, Z) + Noise3D(X - 1, Y, Z) + Noise3D(X, Y + 1, Z);
            corners += Noise3D(X, Y - 1, Z) + Noise3D(X, Y, Z + 1) + Noise3D(X, Y, Z - 1);
            corners /= 16;
            var center = Noise3D(X, Y, Z) / 8;
            return corners + sides + center;
        }

        /*
         * Interpolated Noise Methods
         */
        public double Interpolated2D(double X, double Y)
        {
            //Grid Cell Coordinates
            var X0 = Floor(X);
            var X1 = X0 + 1;
            var Y0 = Floor(Y);
            var Y1 = Y0 + 1;

            //Interpolation weights
            var SX = X - X0;
            var SY = Y - Y0;

            //Interpolate
            var N0 = Smooth2D(X0, Y0);
            var N1 = Smooth2D(X1, Y0);
            var N2 = Smooth2D(X0, Y1);
            var N3 = Smooth2D(X1, Y1);
            var ix0 = Interpolate(N0, N1, SX, Interpolation);
            var ix1 = Interpolate(N2, N3, SX, Interpolation);
            return Interpolate(ix0, ix1, SY, Interpolation);
        }

        public double Interpolated3D(double X, double Y, double Z)
        {
            //Grid Cell Coordinates
            var X0 = Floor(X);
            var X1 = X0 + 1;
            var Y0 = Floor(Y);
            var Y1 = Y0 + 1;
            var Z0 = Floor(Z);
            var Z1 = Z0 + 1;

            //Interpolation weights
            var SX = X - X0;
            var SY = Y - Y0;
            var SZ = Z - Z0;

            //Interpolate
            var N0 = Smooth3D(X0, Y0, Z0);
            var N1 = Smooth3D(X1, Y0, Z0);
            var N2 = Smooth3D(X0, Y1, Z0);
            var N3 = Smooth3D(X1, Y1, Z0);
            var N4 = Smooth3D(X0, Y0, Z1);
            var N5 = Smooth3D(X1, Y0, Z1);
            var N6 = Smooth3D(X0, Y1, Z1);
            var N7 = Smooth3D(X1, Y1, Z1);
            var ix0 = Interpolate(N0, N1, SX, Interpolation);
            var ix1 = Interpolate(N2, N3, SX, Interpolation);
            var ix2 = Interpolate(N4, N5, SX, Interpolation);
            var ix3 = Interpolate(N6, N7, SX, Interpolation);
            var iy0 = Interpolate(ix0, ix1, SY, Interpolation);
            var iy1 = Interpolate(ix2, ix3, SY, Interpolation);
            return Interpolate(iy0, iy1, SZ, Interpolation);
        }
    }
}