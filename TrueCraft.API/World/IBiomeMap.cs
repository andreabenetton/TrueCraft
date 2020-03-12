using System.Collections.Generic;

namespace TrueCraft.API.World
{
    public class BiomeCell
    {
        public byte BiomeID;
        public Coordinates2D CellPoint;

        public BiomeCell(byte biomeID, Coordinates2D cellPoint)
        {
            BiomeID = biomeID;
            CellPoint = cellPoint;
        }
    }

    public interface IBiomeMap
    {
        IList<BiomeCell> BiomeCells { get; }
        void AddCell(BiomeCell cell);
        byte GetBiome(Coordinates2D location);
        byte GenerateBiome(int seed, IBiomeRepository biomes, Coordinates2D location, bool spawn);
        BiomeCell ClosestCell(Coordinates2D location);
        double ClosestCellPoint(Coordinates2D location);
    }
}