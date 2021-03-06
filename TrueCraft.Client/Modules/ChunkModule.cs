﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace TrueCraft.Client.Modules
{
    public class ChunkModule : IGraphicalModule
    {
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
            WorldLighting = new WorldLighting(Game.Client.World.World, Game.BlockRepository);

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

        private BasicEffect OpaqueEffect { get; }
        private AlphaTestEffect TransparentEffect { get; }

        public void Update(GameTime gameTime)
        {
            var any = false;
            while (IncomingChunks.TryTake(out var mesh))
            {
                any = true;
                var chunkMesh = mesh as ChunkMesh;
                if (chunkMesh != null && ActiveMeshes.Contains(chunkMesh.Chunk.Coordinates))
                {
                    var existing = ChunkMeshes.FindIndex(m => m.Chunk.Coordinates == chunkMesh.Chunk.Coordinates);
                    ChunkMeshes[existing] = chunkMesh;
                }
                else
                {
                    if (chunkMesh != null)
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

            var chunks = 0;
            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            foreach (var chunkMesh in ChunkMeshes)
                if (Game.Camera.Frustum.Intersects(chunkMesh.BoundingBox))
                {
                    chunks++;
                    chunkMesh.Draw(OpaqueEffect, 0);
                    if (!chunkMesh.IsReady || chunkMesh.Submeshes != 2)
                        Console.WriteLine("Warning: rendered chunk that was not ready");
                }

            Game.GraphicsDevice.BlendState = ColorWriteDisable;
            foreach (var chunkMesh in ChunkMeshes)
                if (Game.Camera.Frustum.Intersects(chunkMesh.BoundingBox))
                    chunkMesh.Draw(TransparentEffect, 1);

            Game.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            foreach (var chunkMesh in ChunkMeshes)
                if (Game.Camera.Frustum.Intersects(chunkMesh.BoundingBox))
                    chunkMesh.Draw(TransparentEffect, 1);

            ChunksRendered = chunks;
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
                if (adjacent != null)
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
}