using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Client.Rendering;
using TrueCraft.Client.Rendering.Blocks;
using TrueCraft.Core.Logic;
using Xunit;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace Test.TrueCraft.Client
{
    /// <summary>
    ///     Regression guard for per-block allocation pressure. Measures the
    ///     <see cref="GC.GetAllocatedBytesForCurrentThread"/> delta over a fixed number
    ///     of renderer calls.
    ///
    ///     The bound is intentionally loose — this is a regression detector, not a
    ///     micro-benchmark. The pre-refactor baseline for the default renderer is
    ///     ~ several hundred bytes per call; the post-refactor target is "very small,
    ///     ideally bounded by occasional List growth".
    /// </summary>
    public class BlockRendererAllocationTest
    {
        private const int Iterations = 1_000;

        [Fact]
        public void DefaultRendererStaysWithinAllocationBudget()
        {
            // Touch a BlockRenderer subclass so the BlockRenderer static ctor fires
            // and the Renderers[] table is populated.
            _ = typeof(CactusRenderer);

            var repo = new BlockRepository();
            repo.DiscoverBlockProviders();
            var provider = repo.GetBlockProvider(0x01); // Stone — uses the default renderer.
            Assert.NotNull(provider);

            var descriptor = new BlockDescriptor
            {
                ID = 0x01,
                Metadata = 0,
                Coordinates = new Coordinates3D(0, 64, 0),
            };

            // Warm up: JIT + first allocations should not pollute the measurement.
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

            // Loose upper bound: pre-refactor measurement is ~1 KB/call (1 vertex array +
            // 1 index array + scratch). Post-refactor the legacy wrapper will use temp
            // lists internally but the per-call bytes should drop substantially.
            //
            // Today's actual is roughly ~1100 B/call; budget of 1500 keeps the test
            // green pre-refactor and clearly red if a regression doubles allocation.
            Assert.InRange(perCall, 0, 1500.0);
        }
    }
}
