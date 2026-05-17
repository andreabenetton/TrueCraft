using System;
using System.Collections.Generic;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Client.Rendering;
using TrueCraft.Client.Rendering.Blocks;
using TrueCraft.Core.Logic;
using Xunit;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace Test.TrueCraft.Client;

/// <summary>
///     Regression guard for per-block allocation pressure. Measures the
///     <see cref="GC.GetAllocatedBytesForCurrentThread"/> delta over a fixed number
///     of renderer calls.
/// </summary>
public class BlockRendererAllocationTest
{
    private const int Iterations = 1_000;

    /// <summary>
    ///     The new list-appending entry point is what <c>ChunkRenderer</c> uses on
    ///     every block; it must not allocate per call once the destination lists
    ///     are pre-grown.
    /// </summary>
    [Fact]
    public void RenderBlockIntoIsAllocationFreeOnHotPath()
    {
        _ = typeof(CactusRenderer); // Force static ctor.

        var repo = new BlockRepository();
        repo.DiscoverBlockProviders();
        var provider = repo.GetBlockProvider(0x01); // Stone — default renderer.
        Assert.NotNull(provider);

        var descriptor = new BlockDescriptor
        {
            ID = 0x01,
            Metadata = 0,
            Coordinates = new Coordinates3D(0, 64, 0),
        };

        // Pre-grow both lists so list-resize allocations don't pollute the measurement.
        // A full chunk's worth of geometry is well under 1M vertices.
        var vertices = new List<VertexPositionNormalColorTexture>(Iterations * 24 + 64);
        var indices = new List<int>(Iterations * 36 + 64);

        // Warm up.
        for (var i = 0; i < 10; i++)
            BlockRenderer.RenderBlockInto(provider, descriptor, VisibleFaces.All, Vector3.Zero, vertices, indices);

        vertices.Clear();
        indices.Clear();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < Iterations; i++)
            BlockRenderer.RenderBlockInto(provider, descriptor, VisibleFaces.All, Vector3.Zero, vertices, indices);
        var after = GC.GetAllocatedBytesForCurrentThread();

        var perCall = (after - before) / (double) Iterations;

        // Target: 24 bytes/call — the Tuple<int,int> allocated by
        // StoneBlock.GetTextureMap (a separate root cause; tracked
        // for follow-up). The Vector2[4] UV array previously emitted
        // by BlockRenderer.RenderInto is now gone, so this is the
        // only remaining per-call allocation. 40-byte budget gives a
        // small margin and still catches regressions in the UV path.
        Assert.InRange(perCall, 0, 40.0);
    }

    /// <summary>
    ///     The legacy array-returning wrapper (used by <c>IconRenderer</c> and
    ///     <c>HighlightModule</c>) is intentionally allocation-heavy — it builds
    ///     temp lists then ToArray()s. Document the budget separately so it doesn't
    ///     accidentally tighten.
    /// </summary>
    [Fact]
    public void LegacyRenderBlockStaysWithinBudget()
    {
        _ = typeof(CactusRenderer);

        var repo = new BlockRepository();
        repo.DiscoverBlockProviders();
        var provider = repo.GetBlockProvider(0x01);
        Assert.NotNull(provider);

        var descriptor = new BlockDescriptor
        {
            ID = 0x01,
            Metadata = 0,
            Coordinates = new Coordinates3D(0, 64, 0),
        };

        for (var i = 0; i < 10; i++)
            BlockRenderer.RenderBlock(provider, descriptor, VisibleFaces.All, Vector3.Zero, 0, out _);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < Iterations; i++)
            BlockRenderer.RenderBlock(provider, descriptor, VisibleFaces.All, Vector3.Zero, 0, out _);
        var after = GC.GetAllocatedBytesForCurrentThread();

        var perCall = (after - before) / (double) Iterations;

        // Legacy path: 2 lists, 2 ToArray()s, the texture array. ~5000 bytes/call
        // is plausible; budget of 6000 catches catastrophic regressions only.
        Assert.InRange(perCall, 0, 6000.0);
    }
}
