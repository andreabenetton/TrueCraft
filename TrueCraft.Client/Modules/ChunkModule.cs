using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TrueCraft.API;
using TrueCraft.API.World;
using TrueCraft.Client.Events;
using TrueCraft.Client.Rendering;
using TrueCraft.Core.Lighting;
using TrueCraft.Core.World;
using BoundingBox = TrueCraft.API.BoundingBox;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace TrueCraft.Client.Modules;

public class ChunkModule : IGraphicalModule
{
    private static ILogger Log => App.LoggerFor<ChunkModule>();

    private static readonly Coordinates2D[] AdjacentCoordinates =
    {
        Coordinates2D.North, Coordinates2D.South,
        Coordinates2D.East, Coordinates2D.West
    };

    private static readonly BlendState ColorWriteDisable = new BlendState
    {
        ColorWriteChannels = ColorWriteChannels.None
    };

    public ChunkModule(TrueCraftGame game)
    {
        Game = game;

        ChunkRenderer = new ChunkRenderer(Game.Client.World, Game, Game.BlockRepository);
        Game.Client.ChunkLoaded += Game_Client_ChunkLoaded;
        Game.Client.ChunkUnloaded += (sender, e) => UnloadChunk(e.Chunk);
        Game.Client.ChunkModified += Game_Client_ChunkModified;
        Game.Client.BlockChanged += Game_Client_BlockChanged;
        ChunkRenderer.MeshCompleted += MeshCompleted;
        ChunkRenderer.Start();
        WorldLighting = ActivatorUtilities.CreateInstance<WorldLighting>(
            App.Services, Game.Client.World.World, Game.BlockRepository);

        OpaqueEffect = new BasicEffect(Game.GraphicsDevice)
        {
            TextureEnabled = true,
            Texture = Game.TextureMapper.GetTexture("terrain.png"),
            FogEnabled = true,
            FogStart = 0,
            FogEnd = Game.Camera.Frustum.Far.D * 0.8f,
            VertexColorEnabled = true,
            LightingEnabled = true
        };

        TransparentEffect = new AlphaTestEffect(Game.GraphicsDevice)
        {
            AlphaFunction = CompareFunction.Greater,
            ReferenceAlpha = 127,
            Texture = Game.TextureMapper.GetTexture("terrain.png"),
            VertexColorEnabled = true
        };
        OpaqueEffect.LightingEnabled = true;

        ChunkMeshes = new List<ChunkMesh>();
        IncomingChunks = new ConcurrentBag<Mesh>();
        ActiveMeshes = new HashSet<Coordinates2D>();
    }

    public TrueCraftGame Game { get; set; }
    public ChunkRenderer ChunkRenderer { get; set; }
    public int ChunksRendered { get; set; }

    private HashSet<Coordinates2D> ActiveMeshes { get; }
    private List<ChunkMesh> ChunkMeshes { get; }
    private ConcurrentBag<Mesh> IncomingChunks { get; }
    private WorldLighting WorldLighting { get; }

    // Reused per-frame to avoid GC churn from rebuilding the visible-chunk list.
    private readonly List<ChunkMesh> _visibleChunks = new List<ChunkMesh>();

    // Parallel list of per-chunk visible-section bitmasks (bit N == section N
    // passes the frustum test). Indexed in lockstep with _visibleChunks.
    private readonly List<uint> _visibleSectionMasks = new List<uint>();

    private BasicEffect OpaqueEffect { get; }
    private AlphaTestEffect TransparentEffect { get; }

    public void Update(GameTime gameTime)
    {
        var any = false;
        while (IncomingChunks.TryTake(out var mesh))
        {
            any = true;
            var chunkMesh = mesh as ChunkMesh;
            if (chunkMesh is not null && ActiveMeshes.Contains(chunkMesh.Chunk.Coordinates))
            {
                var existing = ChunkMeshes.FindIndex(m => m.Chunk.Coordinates == chunkMesh.Chunk.Coordinates);
                ChunkMeshes[existing] = chunkMesh;
            }
            else
            {
                if (chunkMesh is not null)
                {
                    ActiveMeshes.Add(chunkMesh.Chunk.Coordinates);
                    ChunkMeshes.Add(chunkMesh);
                }
            }
        }

        if (any)
            Game.FlushMainThreadActions();
        WorldLighting.TryLightNext();
    }

    public void Draw(GameTime gameTime)
    {
        OpaqueEffect.FogColor = Game.SkyModule.WorldFogColor.ToVector3();
        Game.Camera.ApplyTo(OpaqueEffect);
        Game.Camera.ApplyTo(TransparentEffect);
        OpaqueEffect.AmbientLightColor = TransparentEffect.DiffuseColor = Color.White.ToVector3()
                                                                          * new Vector3(
                                                                              0.25f + Game.SkyModule
                                                                                  .BrightnessModifier);

        // Two-tier culling: chunk AABB first (fast reject of whole
        // columns out-of-view), then per-section AABB (cull
        // subterranean / above-skyline slabs of nearby chunks). The
        // per-section masks are reused across the three render passes
        // so the per-section frustum test runs only once per chunk.
        var frustum = Game.Camera.Frustum;
        _visibleChunks.Clear();
        _visibleSectionMasks.Clear();
        foreach (var chunkMesh in ChunkMeshes)
        {
            if (!frustum.Intersects(chunkMesh.BoundingBox))
                continue;
            uint mask = 0;
            for (var s = 0; s < ChunkMesh.SectionsPerChunk; s++)
                if (frustum.Intersects(chunkMesh.SectionBounds[s]))
                    mask |= 1u << s;
            if (mask == 0)
                continue;
            _visibleChunks.Add(chunkMesh);
            _visibleSectionMasks.Add(mask);
        }

        Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        for (var c = 0; c < _visibleChunks.Count; c++)
        {
            var chunkMesh = _visibleChunks[c];
            var mask = _visibleSectionMasks[c];
            for (var s = 0; s < ChunkMesh.SectionsPerChunk; s++)
                if ((mask & (1u << s)) != 0)
                    chunkMesh.Draw(OpaqueEffect, ChunkMesh.OpaqueSubmesh(s));
            if (!chunkMesh.IsReady || chunkMesh.Submeshes != ChunkMesh.SectionsPerChunk * 2)
                Log.LogWarning("Rendered chunk that was not ready");
        }

        Game.GraphicsDevice.BlendState = ColorWriteDisable;
        for (var c = 0; c < _visibleChunks.Count; c++)
        {
            var chunkMesh = _visibleChunks[c];
            var mask = _visibleSectionMasks[c];
            for (var s = 0; s < ChunkMesh.SectionsPerChunk; s++)
                if ((mask & (1u << s)) != 0)
                    chunkMesh.Draw(TransparentEffect, ChunkMesh.TransparentSubmesh(s));
        }

        Game.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
        for (var c = 0; c < _visibleChunks.Count; c++)
        {
            var chunkMesh = _visibleChunks[c];
            var mask = _visibleSectionMasks[c];
            for (var s = 0; s < ChunkMesh.SectionsPerChunk; s++)
                if ((mask & (1u << s)) != 0)
                    chunkMesh.Draw(TransparentEffect, ChunkMesh.TransparentSubmesh(s));
        }

        ChunksRendered = _visibleChunks.Count;
    }

    private void Game_Client_BlockChanged(object sender, BlockChangeEventArgs e)
    {
        WorldLighting.EnqueueOperation(new BoundingBox(
            e.Position, e.Position + Coordinates3D.One), false);
        WorldLighting.EnqueueOperation(new BoundingBox(
            e.Position, e.Position + Coordinates3D.One), true);
        var posA = e.Position;
        posA.Y = 0;
        var posB = e.Position;
        posB.Y = World.Height;
        posB.X++;
        posB.Z++;
        WorldLighting.EnqueueOperation(new BoundingBox(posA, posB), true);
        WorldLighting.EnqueueOperation(new BoundingBox(posA, posB), false);
        for (var i = 0; i < 100; i++)
            if (!WorldLighting.TryLightNext())
                break;
    }

    private void Game_Client_ChunkModified(object sender, ChunkEventArgs e)
    {
        ChunkRenderer.Enqueue(e.Chunk, true);
    }

    private void Game_Client_ChunkLoaded(object sender, ChunkEventArgs e)
    {
        ChunkRenderer.Enqueue(e.Chunk);
        foreach (var coordinates in AdjacentCoordinates)
        {
            var adjacent = Game.Client.World.GetChunk(
                coordinates + e.Chunk.Coordinates);
            if (adjacent is not null)
                ChunkRenderer.Enqueue(adjacent);
        }
    }

    private void MeshCompleted(object sender, RendererEventArgs<ReadOnlyChunk> e)
    {
        IncomingChunks.Add(e.Result);
    }

    private void UnloadChunk(ReadOnlyChunk chunk)
    {
        Game.Invoke(() =>
        {
            ActiveMeshes.Remove(chunk.Coordinates);
            ChunkMeshes.RemoveAll(m => m.Chunk.Coordinates == chunk.Coordinates);
        });
    }

    private void HandleClientPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "Position":
                var sorter = new ChunkRenderer.ChunkSorter(new Coordinates3D(
                    (int) Game.Client.Position.X, 0, (int) Game.Client.Position.Z));
                Game.Invoke(() => ChunkMeshes.Sort(sorter));
                break;
        }
    }
}
