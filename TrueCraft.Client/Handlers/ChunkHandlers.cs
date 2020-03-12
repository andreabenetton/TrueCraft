using System;
using MonoGame.Utilities;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.API.Networking;
using TrueCraft.API.World;
using TrueCraft.Client.Events;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.World;

namespace TrueCraft.Client.Handlers
{
    internal static class ChunkHandlers
    {
        public static void HandleBlockChange(IPacket packet, MultiplayerClient client)
        {
            var blockChangePacket = (BlockChangePacket) packet;
            var coordinates = new Coordinates3D(blockChangePacket.X, blockChangePacket.Y, blockChangePacket.Z);
            Coordinates3D adjusted;
            IChunk chunk;
            try
            {
                adjusted = client.World.World.FindBlockPosition(coordinates, out chunk);
            }
            catch (ArgumentException)
            {
                // Relevant chunk is not loaded - ignore packet
                return;
            }

            chunk.SetBlockID(adjusted, (byte) blockChangePacket.BlockID);
            chunk.SetMetadata(adjusted, (byte) blockChangePacket.Metadata);
            client.OnBlockChanged(new BlockChangeEventArgs(coordinates, new BlockDescriptor(),
                new BlockDescriptor()));
            client.OnChunkModified(new ChunkEventArgs(new ReadOnlyChunk(chunk)));
        }

        public static void HandleChunkPreamble(IPacket packet, MultiplayerClient client)
        {
            var chunkPreamblePacket = (ChunkPreamblePacket) packet;
            var coords = new Coordinates2D(chunkPreamblePacket.X, chunkPreamblePacket.Z);
            client.World.SetChunk(coords, new Chunk(coords));
        }

        public static void HandleChunkData(IPacket packet, MultiplayerClient client)
        {
            var chunkDataPacket = (ChunkDataPacket) packet;
            var coords = new Coordinates3D(chunkDataPacket.X, chunkDataPacket.Y, chunkDataPacket.Z);
            var data = ZlibStream.UncompressBuffer(chunkDataPacket.CompressedData);
            var adjustedCoords = client.World.World.FindBlockPosition(coords, out var chunk);

            if (chunkDataPacket.Width == Chunk.Width
                && chunkDataPacket.Height == Chunk.Height
                && chunkDataPacket.Depth == Chunk.Depth) // Fast path
            {
                // Chunk data offsets
                var metadataOffset = chunk.Data.Length;
                var lightOffset = metadataOffset + chunk.Metadata.Length;
                var skylightOffset = lightOffset + chunk.BlockLight.Length;

                // Block IDs
                Buffer.BlockCopy(data, 0, chunk.Data, 0, chunk.Data.Length);
                // Block metadata
                if (metadataOffset < data.Length)
                    Buffer.BlockCopy(data, metadataOffset,
                        chunk.Metadata.Data, 0, chunk.Metadata.Data.Length);
                // Block light
                if (lightOffset < data.Length)
                    Buffer.BlockCopy(data, lightOffset,
                        chunk.BlockLight.Data, 0, chunk.BlockLight.Data.Length);
                // Sky light
                if (skylightOffset < data.Length)
                    Buffer.BlockCopy(data, skylightOffset,
                        chunk.SkyLight.Data, 0, chunk.SkyLight.Data.Length);
            }
            else // Slow path
            {
                int x = adjustedCoords.X, y = adjustedCoords.Y, z = adjustedCoords.Z;
                var fullLength =
                    chunkDataPacket.Width * chunkDataPacket.Height *
                    chunkDataPacket.Depth; // Length of full sized byte section
                var nibbleLength = fullLength / 2; // Length of nibble sections
                for (var i = 0; i < fullLength; i++) // Iterate through block IDs
                {
                    chunk.SetBlockID(new Coordinates3D(x, y, z), data[i]);
                    y++;
                    if (y >= chunkDataPacket.Height)
                    {
                        y = 0;
                        z++;
                        if (z >= chunkDataPacket.Depth)
                        {
                            z = 0;
                            x++;
                            if (x >= chunkDataPacket.Width) x = 0;
                        }
                    }
                }

                x = adjustedCoords.X;
                y = adjustedCoords.Y;
                z = adjustedCoords.Z;
                for (var i = fullLength; i < nibbleLength; i++) // Iterate through metadata
                {
                    var m = data[i];
                    chunk.SetMetadata(new Coordinates3D(x, y, z), (byte) (m & 0xF));
                    chunk.SetMetadata(new Coordinates3D(x, y + 1, z), (byte) (m & (0xF0 << 8)));
                    y += 2;
                    if (y >= chunkDataPacket.Height)
                    {
                        y = 0;
                        z++;
                        if (z >= chunkDataPacket.Depth)
                        {
                            z = 0;
                            x++;
                            if (x >= chunkDataPacket.Width) x = 0;
                        }
                    }
                }

                // TODO: Lighting
            }

            chunk.UpdateHeightMap();
            chunk.TerrainPopulated = true;
            client.OnChunkLoaded(new ChunkEventArgs(new ReadOnlyChunk(chunk)));
        }
    }
}