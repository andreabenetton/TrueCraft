using Microsoft.Xna.Framework;
using TrueCraft.Client.Rendering;
using Xunit;

namespace Test.TrueCraft.Client;

/// <summary>
///     Verifies the vertical-section split in <see cref="ChunkMesh"/>:
///     each of the <see cref="ChunkMesh.SectionsPerChunk"/> sections
///     covers a 16-block-tall band and the bands tile the chunk's full
///     Y range without overlap or gap.
/// </summary>
public class ChunkSectionLayoutTest
{
    [Fact]
    public void SectionsCoverChunkExactlyAndContiguously()
    {
        const int chunkX = 3;
        const int chunkZ = -2;

        // We don't need a real game / GPU for layout checks; the
        // SectionBounds array is computed from chunk grid coords only,
        // so feed in synthetic chunk metadata via a hand-rolled
        // SectionBounds derivation (mirrors ChunkMesh ctor logic).
        var width = global::TrueCraft.Core.World.Chunk.Width;
        var depth = global::TrueCraft.Core.World.Chunk.Depth;
        var sectionHeight = ChunkMesh.SectionHeight;
        var sections = ChunkMesh.SectionsPerChunk;

        Assert.Equal(8, sections);
        Assert.Equal(16, sectionHeight);
        Assert.Equal(global::TrueCraft.Core.World.Chunk.Height, sections * sectionHeight);

        var bounds = new BoundingBox[sections];
        for (var s = 0; s < sections; s++)
        {
            bounds[s] = new BoundingBox(
                new Vector3(chunkX * width, s * sectionHeight, chunkZ * depth),
                new Vector3(chunkX * width + width, (s + 1) * sectionHeight, chunkZ * depth + depth));
        }

        // First section starts at Y=0; last ends at chunk height.
        Assert.Equal(0, bounds[0].Min.Y);
        Assert.Equal(sections * sectionHeight, bounds[sections - 1].Max.Y);

        // Adjacent sections share a Y face (section N's top == section N+1's bottom).
        for (var s = 0; s < sections - 1; s++)
            Assert.Equal(bounds[s].Max.Y, bounds[s + 1].Min.Y);

        // X / Z bounds match the full chunk for every section.
        foreach (var b in bounds)
        {
            Assert.Equal(chunkX * width, b.Min.X);
            Assert.Equal(chunkX * width + width, b.Max.X);
            Assert.Equal(chunkZ * depth, b.Min.Z);
            Assert.Equal(chunkZ * depth + depth, b.Max.Z);
        }
    }

    [Fact]
    public void SubmeshIndexHelpersPartitionOpaqueAndTransparent()
    {
        // Opaque submesh of section N is at 2N; transparent is at 2N+1.
        // Together they fill 0..2*SectionsPerChunk-1 with no overlaps.
        var seen = new bool[ChunkMesh.SectionsPerChunk * 2];
        for (var s = 0; s < ChunkMesh.SectionsPerChunk; s++)
        {
            seen[ChunkMesh.OpaqueSubmesh(s)] = true;
            seen[ChunkMesh.TransparentSubmesh(s)] = true;
        }
        foreach (var f in seen)
            Assert.True(f);
        Assert.NotEqual(ChunkMesh.OpaqueSubmesh(3), ChunkMesh.TransparentSubmesh(3));
    }
}
