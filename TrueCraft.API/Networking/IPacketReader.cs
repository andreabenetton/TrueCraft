using System.Buffers;

namespace TrueCraft.API.Networking;

public interface IPacketReader
{
    int ProtocolVersion { get; }

    void RegisterPacketType<T>(bool clientbound = true, bool serverbound = true) where T : IPacket;
    void WritePacket(IMinecraftStream stream, IPacket packet);

    /// <summary>
    ///     Attempts to read a single packet from the head of <paramref name="buffer"/>. On success,
    ///     <paramref name="buffer"/> is sliced past the consumed bytes and <paramref name="packet"/> is set.
    ///     On insufficient data, returns false with <paramref name="buffer"/> unchanged.
    ///     Throws <see cref="System.NotSupportedException"/> if the leading byte is not a registered packet ID.
    /// </summary>
    bool TryReadPacket(ref ReadOnlySequence<byte> buffer, out IPacket packet, bool serverbound = true);
}
