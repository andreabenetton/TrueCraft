using System;
using System.Collections.ObjectModel;
using TrueCraft.API;
using TrueCraft.API.World;
using TrueCraft.Core.World;

namespace TrueCraft.Client
{
    public class ReadOnlyWorld
    {
        internal ReadOnlyWorld()
        {
            World = new World("default");
            UnloadChunks = true;
        }

        private bool UnloadChunks { get; }

        internal World World { get; set; }

        public long Time => World.Time;

        public byte GetBlockID(Coordinates3D coordinates)
        {
            return World.GetBlockID(coordinates);
        }

        internal void SetBlockID(Coordinates3D coordinates, byte value)
        {
            World.SetBlockID(coordinates, value);
        }

        internal void SetMetadata(Coordinates3D coordinates, byte value)
        {
            World.SetMetadata(coordinates, value);
        }

        public byte GetMetadata(Coordinates3D coordinates)
        {
            return World.GetMetadata(coordinates);
        }

        public byte GetSkyLight(Coordinates3D coordinates)
        {
            return World.GetSkyLight(coordinates);
        }

        internal IChunk FindChunk(Coordinates2D coordinates)
        {
            try
            {
                return World.FindChunk(new Coordinates3D(coordinates.X, 0, coordinates.Z));
            }
            catch
            {
                return null;
            }
        }

        public ReadOnlyChunk GetChunk(Coordinates2D coordinates)
        {
            return new ReadOnlyChunk(World.GetChunk(coordinates));
        }

        internal void SetChunk(Coordinates2D coordinates, Chunk chunk)
        {
            World.SetChunk(coordinates, chunk);
        }

        internal void RemoveChunk(Coordinates2D coordinates)
        {
            if (UnloadChunks)
                World.UnloadChunk(coordinates);
        }

        public bool IsValidPosition(Coordinates3D coords)
        {
            return World.IsValidPosition(coords);
        }
    }

    public class ReadOnlyChunk
    {
        internal ReadOnlyChunk(IChunk chunk)
        {
            Chunk = chunk;
        }

        internal IChunk Chunk { get; set; }

        public Coordinates2D Coordinates => Chunk.Coordinates;

        public int X => Chunk.X;
        public int Z => Chunk.Z;

        public ReadOnlyCollection<byte> Blocks => Array.AsReadOnly(Chunk.Data);
        public ReadOnlyNibbleArray Metadata => new ReadOnlyNibbleArray(Chunk.Metadata);
        public ReadOnlyNibbleArray BlockLight => new ReadOnlyNibbleArray(Chunk.BlockLight);
        public ReadOnlyNibbleArray SkyLight => new ReadOnlyNibbleArray(Chunk.SkyLight);

        public byte GetBlockId(Coordinates3D coordinates)
        {
            return Chunk.GetBlockID(coordinates);
        }

        public byte GetMetadata(Coordinates3D coordinates)
        {
            return Chunk.GetMetadata(coordinates);
        }

        public byte GetSkyLight(Coordinates3D coordinates)
        {
            return Chunk.GetSkyLight(coordinates);
        }

        public byte GetBlockLight(Coordinates3D coordinates)
        {
            return Chunk.GetBlockLight(coordinates);
        }
    }
}