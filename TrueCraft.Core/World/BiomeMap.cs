using System;
using System.Collections.Generic;
using TrueCraft.API;
using TrueCraft.API.World;
using TrueCraft.Core.TerrainGen.Noise;

namespace TrueCraft.Core.World
{
    public class BiomeMap : IBiomeMap
    {
        private readonly Perlin TempNoise;
        private readonly Perlin RainNoise;

        public BiomeMap(int seed)
        {
            TempNoise = new Perlin(seed);
            RainNoise = new Perlin(seed);
            BiomeCells = new List<BiomeCell>();
            TempNoise.Persistance = 1.45;
            TempNoise.Frequency = 0.015;
            TempNoise.Amplitude = 5;
            TempNoise.Octaves = 2;
            TempNoise.Lacunarity = 1.3;
            RainNoise.Frequency = 0.03;
            RainNoise.Octaves = 3;
            RainNoise.Amplitude = 5;
            RainNoise.Lacunarity = 1.7;
            TempNoise.Seed = seed;
            RainNoise.Seed = seed;
        }

        public IList<BiomeCell> BiomeCells { get; }

        public void AddCell(BiomeCell cell)
        {
            BiomeCells.Add(cell);
        }

        public byte GetBiome(Coordinates2D location)
        {
            var BiomeID = ClosestCell(location) != null ? ClosestCell(location).BiomeID : (byte) Biome.Plains;
            return BiomeID;
        }

        public byte GenerateBiome(int seed, IBiomeRepository biomes, Coordinates2D location, bool spawn)
        {
            var temp = Math.Abs(TempNoise.Value2D(location.X, location.Z));
            var rainfall = Math.Abs(RainNoise.Value2D(location.X, location.Z));
            var ID = biomes.GetBiome(temp, rainfall, spawn).ID;
            return ID;
        }

        /*
         * The closest biome cell to the specified location(uses the Chebyshev distance function).
         */
        public BiomeCell ClosestCell(Coordinates2D location)
        {
            BiomeCell cell = null;
            var distance = double.MaxValue;
            foreach (var C in BiomeCells)
            {
                var _distance = Distance(location, C.CellPoint);
                if (_distance < distance)
                {
                    distance = _distance;
                    cell = C;
                }
            }

            return cell;
        }

        /*
         * The distance to the closest biome cell point to the specified location(uses the Chebyshev distance function).
         */
        public double ClosestCellPoint(Coordinates2D location)
        {
            var distance = double.MaxValue;
            foreach (var C in BiomeCells)
            {
                var _distance = Distance(location, C.CellPoint);
                if (_distance < distance) distance = _distance;
            }

            return distance;
        }

        public double Distance(Coordinates2D a, Coordinates2D b)
        {
            var diff = a - b;
            return Math.Max(Math.Abs(diff.X), Math.Abs(diff.Z));
        }
    }
}