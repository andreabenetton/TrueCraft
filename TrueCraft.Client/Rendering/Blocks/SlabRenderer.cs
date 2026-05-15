using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Client.Rendering.Blocks
{
    public class SlabRenderer : BlockRenderer
    {
        private static readonly Vector2 StoneTopTexture = new Vector2(6, 0);
        private static readonly Vector2 StoneSideTexture = new Vector2(5, 0);
        private static readonly Vector2 StoneBottomTexture = new Vector2(6, 0);
        private static readonly Vector2 SandstoneTopTexture = new Vector2(0, 13);
        private static readonly Vector2 SandstoneSideTexture = new Vector2(0, 12);
        private static readonly Vector2 SandstoneBottomTexture = new Vector2(0, 14);
        private static readonly Vector2 WoodTopTexture = new Vector2(4, 0);
        private static readonly Vector2 WoodSideTexture = new Vector2(4, 0);
        private static readonly Vector2 WoodBottomTexture = new Vector2(4, 0);
        private static readonly Vector2 CobbleTopTexture = new Vector2(0, 1);
        private static readonly Vector2 CobbleSideTexture = new Vector2(0, 1);
        private static readonly Vector2 CobbleBottomTexture = new Vector2(0, 1);

        private static readonly Vector2[] StoneTextureMap =
        {
            // Positive Z
            StoneSideTexture + Vector2.UnitX + Vector2.UnitY,
            StoneSideTexture + Vector2.UnitY,
            StoneSideTexture,
            StoneSideTexture + Vector2.UnitX,
            // Negative Z
            StoneSideTexture + Vector2.UnitX + Vector2.UnitY,
            StoneSideTexture + Vector2.UnitY,
            StoneSideTexture,
            StoneSideTexture + Vector2.UnitX,
            // Positive X
            StoneSideTexture + Vector2.UnitX + Vector2.UnitY,
            StoneSideTexture + Vector2.UnitY,
            StoneSideTexture,
            StoneSideTexture + Vector2.UnitX,
            // Negative X
            StoneSideTexture + Vector2.UnitX + Vector2.UnitY,
            StoneSideTexture + Vector2.UnitY,
            StoneSideTexture,
            StoneSideTexture + Vector2.UnitX,
            // Negative Y
            StoneTopTexture + Vector2.UnitX + Vector2.UnitY,
            StoneTopTexture + Vector2.UnitY,
            StoneTopTexture,
            StoneTopTexture + Vector2.UnitX,
            // Negative Y
            StoneBottomTexture + Vector2.UnitX + Vector2.UnitY,
            StoneBottomTexture + Vector2.UnitY,
            StoneBottomTexture,
            StoneBottomTexture + Vector2.UnitX
        };

        private static readonly Vector2[] SandstoneTextureMap =
        {
            // Positive Z
            SandstoneSideTexture + Vector2.UnitX + Vector2.UnitY,
            SandstoneSideTexture + Vector2.UnitY,
            SandstoneSideTexture,
            SandstoneSideTexture + Vector2.UnitX,
            // Negative Z
            SandstoneSideTexture + Vector2.UnitX + Vector2.UnitY,
            SandstoneSideTexture + Vector2.UnitY,
            SandstoneSideTexture,
            SandstoneSideTexture + Vector2.UnitX,
            // Positive X
            SandstoneSideTexture + Vector2.UnitX + Vector2.UnitY,
            SandstoneSideTexture + Vector2.UnitY,
            SandstoneSideTexture,
            SandstoneSideTexture + Vector2.UnitX,
            // Negative X
            SandstoneSideTexture + Vector2.UnitX + Vector2.UnitY,
            SandstoneSideTexture + Vector2.UnitY,
            SandstoneSideTexture,
            SandstoneSideTexture + Vector2.UnitX,
            // Negative Y
            SandstoneTopTexture + Vector2.UnitX + Vector2.UnitY,
            SandstoneTopTexture + Vector2.UnitY,
            SandstoneTopTexture,
            SandstoneTopTexture + Vector2.UnitX,
            // Negative Y
            SandstoneBottomTexture + Vector2.UnitX + Vector2.UnitY,
            SandstoneBottomTexture + Vector2.UnitY,
            SandstoneBottomTexture,
            SandstoneBottomTexture + Vector2.UnitX
        };

        private static readonly Vector2[] WoodTextureMap =
        {
            // Positive Z
            WoodSideTexture + Vector2.UnitX + Vector2.UnitY,
            WoodSideTexture + Vector2.UnitY,
            WoodSideTexture,
            WoodSideTexture + Vector2.UnitX,
            // Negative Z
            WoodSideTexture + Vector2.UnitX + Vector2.UnitY,
            WoodSideTexture + Vector2.UnitY,
            WoodSideTexture,
            WoodSideTexture + Vector2.UnitX,
            // Positive X
            WoodSideTexture + Vector2.UnitX + Vector2.UnitY,
            WoodSideTexture + Vector2.UnitY,
            WoodSideTexture,
            WoodSideTexture + Vector2.UnitX,
            // Negative X
            WoodSideTexture + Vector2.UnitX + Vector2.UnitY,
            WoodSideTexture + Vector2.UnitY,
            WoodSideTexture,
            WoodSideTexture + Vector2.UnitX,
            // Negative Y
            WoodTopTexture + Vector2.UnitX + Vector2.UnitY,
            WoodTopTexture + Vector2.UnitY,
            WoodTopTexture,
            WoodTopTexture + Vector2.UnitX,
            // Negative Y
            WoodBottomTexture + Vector2.UnitX + Vector2.UnitY,
            WoodBottomTexture + Vector2.UnitY,
            WoodBottomTexture,
            WoodBottomTexture + Vector2.UnitX
        };

        private static readonly Vector2[] CobbleTextureMap =
        {
            // Positive Z
            CobbleSideTexture + Vector2.UnitX + Vector2.UnitY,
            CobbleSideTexture + Vector2.UnitY,
            CobbleSideTexture,
            CobbleSideTexture + Vector2.UnitX,
            // Negative Z
            CobbleSideTexture + Vector2.UnitX + Vector2.UnitY,
            CobbleSideTexture + Vector2.UnitY,
            CobbleSideTexture,
            CobbleSideTexture + Vector2.UnitX,
            // Positive X
            CobbleSideTexture + Vector2.UnitX + Vector2.UnitY,
            CobbleSideTexture + Vector2.UnitY,
            CobbleSideTexture,
            CobbleSideTexture + Vector2.UnitX,
            // Negative X
            CobbleSideTexture + Vector2.UnitX + Vector2.UnitY,
            CobbleSideTexture + Vector2.UnitY,
            CobbleSideTexture,
            CobbleSideTexture + Vector2.UnitX,
            // Negative Y
            CobbleTopTexture + Vector2.UnitX + Vector2.UnitY,
            CobbleTopTexture + Vector2.UnitY,
            CobbleTopTexture,
            CobbleTopTexture + Vector2.UnitX,
            // Negative Y
            CobbleBottomTexture + Vector2.UnitX + Vector2.UnitY,
            CobbleBottomTexture + Vector2.UnitY,
            CobbleBottomTexture,
            CobbleBottomTexture + Vector2.UnitX
        };

        static SlabRenderer()
        {
            RegisterRenderer(SlabBlock.BlockID, new SlabRenderer());
            RegisterRenderer(DoubleSlabBlock.BlockID, new SlabRenderer());

            for (var i = 0; i < StoneTextureMap.Length; i++)
            {
                StoneTextureMap[i] *= new Vector2(16f / 256f);
                SandstoneTextureMap[i] *= new Vector2(16f / 256f);
                WoodTextureMap[i] *= new Vector2(16f / 256f);
                CobbleTextureMap[i] *= new Vector2(16f / 256f);
            }
        }

        protected virtual Vector2[] GetTextureMap(SlabBlock.SlabMaterial material)
        {
            switch (material)
            {
                case SlabBlock.SlabMaterial.Stone:
                    return StoneTextureMap;
                case SlabBlock.SlabMaterial.Standstone:
                    return SandstoneTextureMap;
                case SlabBlock.SlabMaterial.Wooden:
                    return WoodTextureMap;
                case SlabBlock.SlabMaterial.Cobblestone:
                    return CobbleTextureMap;
                default:
                    return StoneTextureMap;
            }
        }

        public override void RenderInto(BlockDescriptor descriptor, Vector3 offset, VisibleFaces faces,
            Tuple<int, int> textureMap,
            List<VertexPositionNormalColorTexture> vertices, List<int> indices)
        {
            if (descriptor.ID == SlabBlock.BlockID)
                RenderSlabInto(descriptor, offset, vertices, indices);
            else
                RenderDoubleSlabInto(descriptor, offset, vertices, indices);
        }

        protected virtual void RenderSlabInto(BlockDescriptor descriptor, Vector3 offset,
            List<VertexPositionNormalColorTexture> vertices, List<int> indices)
        {
            Span<int> lighting = stackalloc int[6];
            for (var i = 0; i < 6; i++)
                lighting[i] = GetLight(descriptor.Chunk, descriptor.Coordinates + FaceCoords[i]);

            var start = vertices.Count;
            CreateUniformCubeInto(offset,
                GetTextureMap((SlabBlock.SlabMaterial) descriptor.Metadata), VisibleFaces.All,
                Color.White, lighting, vertices, indices);

            var span = CollectionsMarshal.AsSpan(vertices).Slice(start);
            for (var i = 0; i < 6; i++)
            {
                var face = (CubeFace) i;
                switch (face)
                {
                    case CubeFace.PositiveZ:
                    case CubeFace.NegativeZ:
                    case CubeFace.PositiveX:
                    case CubeFace.NegativeX:
                        for (var j = 0; j < 2; j++)
                            span[i * 4 + j].Texture.Y -= 1f / 32f;
                        for (var k = 2; k < 4; k++)
                            span[i * 4 + k].Position.Y -= 0.5f;
                        break;

                    case CubeFace.PositiveY:
                        for (var j = 0; j < 4; j++)
                            span[i * 4 + j].Position.Y -= 0.5f;
                        break;
                }
            }
        }

        protected virtual void RenderDoubleSlabInto(BlockDescriptor descriptor, Vector3 offset,
            List<VertexPositionNormalColorTexture> vertices, List<int> indices)
        {
            ReadOnlySpan<int> defaultLighting = DefaultLighting;
            CreateUniformCubeInto(offset, GetTextureMap((SlabBlock.SlabMaterial) descriptor.Metadata),
                VisibleFaces.All, Color.White, defaultLighting, vertices, indices);
        }
    }
}