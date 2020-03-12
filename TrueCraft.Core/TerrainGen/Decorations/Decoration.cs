using System;
using TrueCraft.API;
using TrueCraft.API.World;
using TrueCraft.Core.World;

namespace TrueCraft.Core.TerrainGen.Decorations
{
    public abstract class Decoration : IDecoration
    {
        public virtual bool ValidLocation(Coordinates3D location)
        {
            return true;
        }

        public abstract bool GenerateAt(IWorld world, IChunk chunk, Coordinates3D location);

        public static bool IsCuboidWall(Coordinates2D location, Coordinates3D start, Vector3 size)
        {
            return location.X.Equals(start.X)
                   || location.Z.Equals(start.Z)
                   || location.X.Equals(start.X + (int) size.X - 1)
                   || location.Z.Equals(start.Z + (int) size.Z - 1);
        }

        public static bool IsCuboidCorner(Coordinates2D location, Coordinates3D start, Vector3 size)
        {
            return location.X.Equals(start.X) && location.Z.Equals(start.Z)
                   || location.X.Equals(start.X) && location.Z.Equals(start.Z + (int) size.Z - 1)
                   || location.X.Equals(start.X + (int) size.X - 1) && location.Z.Equals(start.Z)
                   || location.X.Equals(start.X + (int) size.X - 1) && location.Z.Equals(start.Z + (int) size.Z - 1);
        }

        public static bool NeighboursBlock(IChunk chunk, Coordinates3D location, byte block, byte meta = 0x0)
        {
            var surrounding = new[]
            {
                location + Coordinates3D.Left,
                location + Coordinates3D.Right,
                location + Coordinates3D.Forwards,
                location + Coordinates3D.Backwards
            };
            for (var i = 0; i < surrounding.Length; i++)
            {
                var toCheck = surrounding[i];
                if (toCheck.X < 0 || toCheck.X >= Chunk.Width || toCheck.Z < 0 || toCheck.Z >= Chunk.Depth ||
                    toCheck.Y < 0 || toCheck.Y >= Chunk.Height)
                    return false;
                if (chunk.GetBlockID(toCheck).Equals(block))
                {
                    if (meta != 0x0 && chunk.GetMetadata(toCheck) != meta)
                        return false;
                    return true;
                }
            }

            return false;
        }

        public static void GenerateColumn(IChunk chunk, Coordinates3D location, int height, byte block, byte meta = 0x0)
        {
            for (var offset = 0; offset < height; offset++)
            {
                var blockLocation = location + new Coordinates3D(0, offset);
                if (blockLocation.Y >= Chunk.Height)
                    return;
                chunk.SetBlockID(blockLocation, block);
                chunk.SetMetadata(blockLocation, meta);
            }
        }

        /*
         * Cuboid Modes
         * 0x0 - Solid cuboid of the specified block
         * 0x1 - Hollow cuboid of the specified block
         * 0x2 - Outlines the area of the cuboid using the specified block
         */
        public static void GenerateCuboid(IChunk chunk, Coordinates3D location, Vector3 size, byte block,
            byte meta = 0x0, byte mode = 0x0)
        {
            //If mode is 0x2 offset the size by 2 and change mode to 0x1
            if (mode.Equals(0x2))
            {
                size += new Vector3(2, 2, 2);
                mode = 0x1;
            }

            for (var w = location.X; w < location.X + size.X; w++)
            for (var l = location.Z; l < location.Z + size.Z; l++)
            for (var h = location.Y; h < location.Y + size.Y; h++)
            {
                if (w < 0 || w >= Chunk.Width || l < 0 || l >= Chunk.Depth || h < 0 || h >= Chunk.Height)
                    continue;
                var BlockLocation = new Coordinates3D(w, h, l);
                if (!h.Equals(location.Y) && !h.Equals(location.Y + (int) size.Y - 1)
                                          && !IsCuboidWall(new Coordinates2D(w, l), location, size)
                                          && !IsCuboidCorner(new Coordinates2D(w, l), location, size))
                    continue;

                chunk.SetBlockID(BlockLocation, block);
                if (meta != 0x0)
                    chunk.SetMetadata(BlockLocation, meta);
            }
        }

        protected void GenerateVanillaLeaves(IChunk chunk, Coordinates3D location, int radius, byte block,
            byte meta = 0x0)
        {
            var radiusOffset = radius;
            for (var yOffset = -radius; yOffset <= radius; yOffset = yOffset + 1)
            {
                var y = location.Y + yOffset;
                if (y > Chunk.Height)
                    continue;
                GenerateVanillaCircle(chunk, new Coordinates3D(location.X, y, location.Z), radiusOffset, block, meta);
                if (yOffset != -radius && yOffset % 2 == 0)
                    radiusOffset--;
            }
        }

        protected void GenerateVanillaCircle(IChunk chunk, Coordinates3D location, int radius, byte block,
            byte meta = 0x0, double corner = 0)
        {
            for (var i = -radius; i <= radius; i = i + 1)
            for (var j = -radius; j <= radius; j = j + 1)
            {
                var max = (int) Math.Sqrt(i * i + j * j);
                if (max <= radius)
                {
                    if (i.Equals(-radius) && j.Equals(-radius)
                        || i.Equals(-radius) && j.Equals(radius)
                        || i.Equals(radius) && j.Equals(-radius)
                        || i.Equals(radius) && j.Equals(radius))
                        if (corner + radius * 0.2 < 0.4 || corner + radius * 0.2 > 0.7 || corner.Equals(0))
                            continue;
                    var x = location.X + i;
                    var z = location.Z + j;
                    var currentBlock = new Coordinates3D(x, location.Y, z);
                    if (chunk.GetBlockID(currentBlock).Equals(0))
                    {
                        chunk.SetBlockID(currentBlock, block);
                        chunk.SetMetadata(currentBlock, meta);
                    }
                }
            }
        }

        protected void GenerateCircle(IChunk chunk, Coordinates3D location, int radius, byte block, byte meta = 0x0)
        {
            for (var i = -radius; i <= radius; i = i + 1)
            for (var j = -radius; j <= radius; j = j + 1)
            {
                var max = (int) Math.Sqrt(i * i + j * j);
                if (max <= radius)
                {
                    var x = location.X + i;
                    var z = location.Z + j;

                    if (x < 0 || x >= Chunk.Width || z < 0 || z >= Chunk.Depth)
                        continue;

                    var currentBlock = new Coordinates3D(x, location.Y, z);
                    if (chunk.GetBlockID(currentBlock).Equals(0))
                    {
                        chunk.SetBlockID(currentBlock, block);
                        chunk.SetMetadata(currentBlock, meta);
                    }
                }
            }
        }

        protected static void GenerateSphere(IChunk chunk, Coordinates3D location, int radius, byte block,
            byte meta = 0x0)
        {
            for (var i = -radius; i <= radius; i = i + 1)
            for (var j = -radius; j <= radius; j = j + 1)
            for (var k = -radius; k <= radius; k = k + 1)
            {
                var max = (int) Math.Sqrt(i * i + j * j + k * k);
                if (max <= radius)
                {
                    var x = location.X + i;
                    var y = location.Y + k;
                    var z = location.Z + j;

                    if (x < 0 || x >= Chunk.Width || z < 0 || z >= Chunk.Depth || y < 0 || y >= Chunk.Height)
                        continue;

                    var currentBlock = new Coordinates3D(x, y, z);
                    if (chunk.GetBlockID(currentBlock).Equals(0))
                    {
                        chunk.SetBlockID(currentBlock, block);
                        chunk.SetMetadata(currentBlock, meta);
                    }
                }
            }
        }
    }
}