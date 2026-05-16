using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.World;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace TrueCraft.Client.Rendering.Blocks;

public class GrassRenderer : BlockRenderer
{
    private static readonly Vector2 TextureMap = new Vector2(0, 0);
    private static readonly Vector2 EndsTexture = new Vector2(2, 0);
    private static readonly Vector2 SideTexture = new Vector2(3, 0);
    private static readonly Vector2 SideTextureSnow = new Vector2(4, 4);

    private static readonly Vector2[] Texture =
    {
        // Positive Z
        SideTexture + Vector2.UnitX + Vector2.UnitY,
        SideTexture + Vector2.UnitY,
        SideTexture,
        SideTexture + Vector2.UnitX,
        // Negative Z
        SideTexture + Vector2.UnitX + Vector2.UnitY,
        SideTexture + Vector2.UnitY,
        SideTexture,
        SideTexture + Vector2.UnitX,
        // Positive X
        SideTexture + Vector2.UnitX + Vector2.UnitY,
        SideTexture + Vector2.UnitY,
        SideTexture,
        SideTexture + Vector2.UnitX,
        // Negative X
        SideTexture + Vector2.UnitX + Vector2.UnitY,
        SideTexture + Vector2.UnitY,
        SideTexture,
        SideTexture + Vector2.UnitX,
        // Positive Y
        TextureMap + Vector2.UnitX + Vector2.UnitY,
        TextureMap + Vector2.UnitY,
        TextureMap,
        TextureMap + Vector2.UnitX,
        // Negative Y
        EndsTexture + Vector2.UnitX + Vector2.UnitY,
        EndsTexture + Vector2.UnitY,
        EndsTexture,
        EndsTexture + Vector2.UnitX
    };

    private static readonly Vector2[] SnowTexture =
    {
        // Positive Z
        SideTextureSnow + Vector2.UnitX + Vector2.UnitY,
        SideTextureSnow + Vector2.UnitY,
        SideTextureSnow,
        SideTextureSnow + Vector2.UnitX,
        // Negative Z
        SideTextureSnow + Vector2.UnitX + Vector2.UnitY,
        SideTextureSnow + Vector2.UnitY,
        SideTextureSnow,
        SideTextureSnow + Vector2.UnitX,
        // Positive X
        SideTextureSnow + Vector2.UnitX + Vector2.UnitY,
        SideTextureSnow + Vector2.UnitY,
        SideTextureSnow,
        SideTextureSnow + Vector2.UnitX,
        // Negative X
        SideTextureSnow + Vector2.UnitX + Vector2.UnitY,
        SideTextureSnow + Vector2.UnitY,
        SideTextureSnow,
        SideTextureSnow + Vector2.UnitX,
        // Positive Y
        TextureMap + Vector2.UnitX + Vector2.UnitY,
        TextureMap + Vector2.UnitY,
        TextureMap,
        TextureMap + Vector2.UnitX,
        // Negative Y
        EndsTexture + Vector2.UnitX + Vector2.UnitY,
        EndsTexture + Vector2.UnitY,
        EndsTexture,
        EndsTexture + Vector2.UnitX
    };

    public static readonly Color BiomeColor = new Color(105, 169, 63);

    static GrassRenderer()
    {
        RegisterRenderer(GrassBlock.BlockID, new GrassRenderer());
        for (var i = 0; i < Texture.Length; i++)
            Texture[i] *= new Vector2(16f / 256f);
        for (var i = 0; i < Texture.Length; i++)
            SnowTexture[i] *= new Vector2(16f / 256f);
    }

    public override void RenderInto(BlockDescriptor descriptor, Vector3 offset, VisibleFaces faces,
        Tuple<int, int> textureMap,
        List<VertexPositionNormalColorTexture> vertices, List<int> indices)
    {
        var texture = Texture;
        if (descriptor.Coordinates.Y < World.Height && descriptor.Chunk is not null)
            if (descriptor.Chunk.GetBlockID(descriptor.Coordinates + Coordinates3D.Up) == SnowfallBlock.BlockID)
                texture = SnowTexture;

        Span<int> lighting = stackalloc int[6];
        for (var i = 0; i < 6; i++)
            lighting[i] = GetLight(descriptor.Chunk, descriptor.Coordinates + FaceCoords[i]);

        var start = vertices.Count;
        CreateUniformCubeInto(offset, texture, faces, Color.White, lighting, vertices, indices);
        // Apply biome colors to top of cube (PositiveY face = 4; 4 verts per face).
        var span = CollectionsMarshal.AsSpan(vertices).Slice(start);
        for (var i = (int) CubeFace.PositiveY * 4; i < (int) CubeFace.PositiveY * 4 + 4 && i < span.Length; i++)
            span[i].Color =
                new Color(span[i].Color.ToVector3() * BiomeColor.ToVector3()); // TODO: Take this from biome
    }
}
