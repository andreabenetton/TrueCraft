using System.Buffers;
using Microsoft.Xna.Framework;

namespace TrueCraft.Client.Rendering;

/// <summary>
///     A mesh covering one full chunk's worth of geometry. Internally the
///     index data is partitioned into <see cref="SectionsPerChunk"/> vertical
///     16-block slabs so the renderer can frustum-cull subterranean /
///     above-the-skyline sections of distant chunks before issuing their
///     draw calls. The vertex buffer is shared across all sections;
///     each section just owns its slice of indices.
///
///     Submesh layout (0..2*SectionsPerChunk-1):
///       opaque-of-section-N      at index N*2
///       transparent-of-section-N at index N*2 + 1
///
///     <para>
///         The vertex and index arrays come from
///         <see cref="ChunkRenderer"/>'s pool-backed <see cref="Buffer{T}"/>s
///         and may be larger than their live element counts.
///         <see cref="Mesh.SetVertices"/> and
///         <see cref="Mesh.SetSubmesh(int, int[], int)"/> upload only the
///         live prefix; <see cref="Dispose"/> returns the arrays to
///         <see cref="ArrayPool{T}.Shared"/>.
///     </para>
/// </summary>
public class ChunkMesh : Mesh
{
    public const int SectionsPerChunk = 8;
    public const int SectionHeight = 16;

    private readonly VertexPositionNormalColorTexture[] _ownedVerts;
    private readonly int[][] _ownedOpaqueIndices;
    private readonly int[][] _ownedTransparentIndices;

    /// <summary>
    ///     Bounding box of each vertical section, in world space.
    ///     Indexed 0..SectionsPerChunk-1 (Y ascending).
    /// </summary>
    public readonly BoundingBox[] SectionBounds;

    public ChunkMesh(ReadOnlyChunk chunk, TrueCraftGame game,
        VertexPositionNormalColorTexture[] vertices, int vertexCount,
        int[][] opaqueIndicesPerSection, int[] opaqueCountsPerSection,
        int[][] transparentIndicesPerSection, int[] transparentCountsPerSection)
        : base(game, SectionsPerChunk * 2)
    {
        Chunk = chunk;
        _ownedVerts = vertices;
        _ownedOpaqueIndices = opaqueIndicesPerSection;
        _ownedTransparentIndices = transparentIndicesPerSection;

        SectionBounds = new BoundingBox[SectionsPerChunk];
        var width = Core.World.Chunk.Width;
        var depth = Core.World.Chunk.Depth;
        for (var s = 0; s < SectionsPerChunk; s++)
        {
            SectionBounds[s] = new BoundingBox(
                new Vector3(chunk.X * width, s * SectionHeight, chunk.Z * depth),
                new Vector3(chunk.X * width + width, (s + 1) * SectionHeight, chunk.Z * depth + depth));
        }

        SetVertices(vertices, vertexCount);
        for (var s = 0; s < SectionsPerChunk; s++)
        {
            SetSubmesh(s * 2, opaqueIndicesPerSection[s], opaqueCountsPerSection[s]);
            SetSubmesh(s * 2 + 1, transparentIndicesPerSection[s], transparentCountsPerSection[s]);
        }
    }

    public ReadOnlyChunk Chunk { get; set; }

    /// <summary>Submesh index of section <paramref name="s"/>'s opaque indices.</summary>
    public static int OpaqueSubmesh(int s) => s * 2;

    /// <summary>Submesh index of section <paramref name="s"/>'s transparent indices.</summary>
    public static int TransparentSubmesh(int s) => s * 2 + 1;

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
                for (var s = 0; s < _ownedOpaqueIndices.Length; s++)
                    if (_ownedOpaqueIndices[s] is not null)
                        ArrayPool<int>.Shared.Return(_ownedOpaqueIndices[s]);
            if (_ownedTransparentIndices is not null)
                for (var s = 0; s < _ownedTransparentIndices.Length; s++)
                    if (_ownedTransparentIndices[s] is not null)
                        ArrayPool<int>.Shared.Return(_ownedTransparentIndices[s]);
        }

        base.Dispose(disposing);
    }
}
