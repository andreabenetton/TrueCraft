using System;
using TrueCraft.API;
using TrueCraft.API.World;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Core.TerrainGen.Decorations
{
    public class ConiferTree : PineTree
    {
        private const int LeafRadius = 2;

        public override bool GenerateAt(IWorld world, IChunk chunk, Coordinates3D location)
        {
            if (!ValidLocation(location))
                return false;

            var random = new Random(world.Seed);
            var height = random.Next(7, 8);
            GenerateColumn(chunk, location, height, WoodBlock.BlockID, 0x1);
            GenerateCircle(chunk, location + new Coordinates3D(0, height - 2), LeafRadius - 1, LeavesBlock.BlockID,
                0x1);
            GenerateCircle(chunk, location + new Coordinates3D(0, height - 1), LeafRadius, LeavesBlock.BlockID, 0x1);
            GenerateCircle(chunk, location + new Coordinates3D(0, height), LeafRadius, LeavesBlock.BlockID, 0x1);
            GenerateTopper(chunk, location + new Coordinates3D(0, height + 1));
            return true;
        }
    }
}