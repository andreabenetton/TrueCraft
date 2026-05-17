using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Client.Rendering.Blocks;

public class LeavesRenderer : BlockRenderer
{
    private static readonly Vector2 BaseTexture = new Vector2(4, 3);
    private static readonly Vector2 SpruceTexture = new Vector2(4, 8);

    private static readonly Vector2[] BaseTextures =
    {
        BaseTexture + Vector2.UnitX + Vector2.UnitY,
        BaseTexture + Vector2.UnitY,
        BaseTexture,
        BaseTexture + Vector2.UnitX
    };

    private static readonly Vector2[] SpruceTextures =
    {
        SpruceTexture + Vector2.UnitX + Vector2.UnitY,
        SpruceTexture + Vector2.UnitY,
        SpruceTexture,
        SpruceTexture + Vector2.UnitX
    };

    static LeavesRenderer()
    {
        RegisterRenderer(LeavesBlock.BlockID, new LeavesRenderer());
        for (var i = 0; i < BaseTextures.Length; i++)
        {
            BaseTextures[i] *= new Vector2(16f / 256f);
            SpruceTextures[i] *= new Vector2(16f / 256f);
        }
    }

    public override void RenderInto(BlockDescriptor descriptor, Vector3 offset, VisibleFaces faces,
        Tuple<int, int> textureMap,
        Buffer<VertexPositionNormalColorTexture> vertices, Buffer<int> indices)
    {
        Span<int> lighting = stackalloc int[6];
        for (var i = 0; i < 6; i++)
            lighting[i] = GetLight(descriptor.Chunk, descriptor.Coordinates + FaceCoords[i]);

        Vector2[] texture;
        switch ((WoodBlock.WoodType) descriptor.Metadata)
        {
            case WoodBlock.WoodType.Spruce:
                texture = SpruceTextures;
                break;
            case WoodBlock.WoodType.Birch:
            case WoodBlock.WoodType.Oak:
            default:
                texture = BaseTextures;
                break;
        }

        CreateUniformCubeInto(offset, texture, VisibleFaces.All,
            GrassRenderer.BiomeColor, lighting, vertices, indices);
    }
}
