using System.Buffers;
using Microsoft.Xna.Framework;

namespace TrueCraft.Client.Rendering;

/// <summary>
///     A mesh covering one full chunk's worth of geometry.
///
///     <para>
///         The vertex and index arrays are taken by ownership transfer from
///         <see cref="ChunkRenderer"/>'s pool-backed <see cref="Buffer{T}"/>s
///         — they may be larger than the "live" element count, with the
///         remainder uninitialised. <see cref="Mesh.SetVertices"/> and
///         <see cref="Mesh.SetSubmesh(int, int[], int)"/> upload only the
///         live prefix.
///     </para>
///     <para>
///         <see cref="Dispose"/> returns the arrays to
///         <see cref="ArrayPool{T}.Shared"/> so the next chunk to be meshed
///         on a worker thread re-uses them.
///     </para>
/// </summary>
public class ChunkMesh : Mesh
{
    private readonly VertexPositionNormalColorTexture[] _ownedVerts;
    private readonly int[] _ownedOpaqueIndices;
    private readonly int[] _ownedTransparentIndices;

    public ChunkMesh(ReadOnlyChunk chunk, TrueCraftGame game,
        VertexPositionNormalColorTexture[] vertices, int vertexCount,
        int[] opaqueIndices, int opaqueCount,
        int[] transparentIndices, int transparentCount)
        : base(game, 2)
    {
        Chunk = chunk;
        _ownedVerts = vertices;
        _ownedOpaqueIndices = opaqueIndices;
        _ownedTransparentIndices = transparentIndices;
        SetVertices(vertices, vertexCount);
        SetSubmesh(0, opaqueIndices, opaqueCount);
        SetSubmesh(1, transparentIndices, transparentCount);
    }

    public ReadOnlyChunk Chunk { get; set; }

    /// <summary>
    ///     Bounds are derived from the chunk's grid position, not its
    ///     vertex data — the chunk's AABB is independent of how many
    ///     blocks happen to be visible.
    /// </summary>
    protected override BoundingBox RecalculateBounds(VertexPositionNormalColorTexture[] vertices, int count)
    {
        return new BoundingBox(
            new Vector3(Chunk.X * Core.World.Chunk.Width, 0, Chunk.Z * Core.World.Chunk.Depth),
            new Vector3(Chunk.X * Core.World.Chunk.Width
                        + Core.World.Chunk.Width, Core.World.Chunk.Height,
                Chunk.Z * Core.World.Chunk.Depth + Core.World.Chunk.Depth));
    }

    protected override void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            base.Dispose(disposing);
            return;
        }

        if (disposing)
        {
            if (_ownedVerts is not null)
                ArrayPool<VertexPositionNormalColorTexture>.Shared.Return(_ownedVerts);
            if (_ownedOpaqueIndices is not null)
                ArrayPool<int>.Shared.Return(_ownedOpaqueIndices);
            if (_ownedTransparentIndices is not null)
                ArrayPool<int>.Shared.Return(_ownedTransparentIndices);
        }

        base.Dispose(disposing);
    }
}
