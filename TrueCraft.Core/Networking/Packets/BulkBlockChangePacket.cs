﻿using TrueCraft.API;
using TrueCraft.API.Networking;

namespace TrueCraft.Core.Networking.Packets
{
    public struct BulkBlockChangePacket : IPacket
    {
        public byte ID => 0x34;

        public int ChunkX, ChunkZ;
        public Coordinates3D[] Coordinates;
        public sbyte[] BlockIDs;
        public sbyte[] Metadata;

        public void ReadPacket(IMinecraftStream stream)
        {
            ChunkX = stream.ReadInt32();
            ChunkZ = stream.ReadInt32();
            var length = stream.ReadInt16();
            Coordinates = new Coordinates3D[length];
            for (var i = 0; i < length; i++)
            {
                var value = stream.ReadUInt16();
                Coordinates[i] = new Coordinates3D(
                    (value >> 12) & 0xF,
                    value & 0xFF,
                    (value >> 8) & 0xF);
            }

            BlockIDs = stream.ReadInt8Array(length);
            Metadata = stream.ReadInt8Array(length);
        }

        public void WritePacket(IMinecraftStream stream)
        {
            stream.WriteInt32(ChunkX);
            stream.WriteInt32(ChunkZ);
            stream.WriteInt16((short) Coordinates.Length);
            foreach (var coordinate in Coordinates)
                stream.WriteUInt16((ushort) (((coordinate.X << 12) & 0xF) | ((coordinate.Z << 8) & 0xF) |
                                             (coordinate.Y & 0xFF)));
            stream.WriteInt8Array(BlockIDs);
            stream.WriteInt8Array(Metadata);
        }
    }
}