using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Client.Rendering.Blocks;

public class SnowRenderer : BlockRenderer
{
    private static readonly Vector2 TextureMap = new Vector2(2, 4);

    private static readonly Vector2[] Texture =
    {
        TextureMap + Vector2.UnitX + Vector2.UnitY,
        TextureMap + Vector2.UnitY,
        TextureMap,
        TextureMap + Vector2.UnitX
    };

    static SnowRenderer()
    {
        RegisterRenderer(SnowfallBlock.BlockID, new SnowRenderer());
        for (var i = 0; i < Texture.Length; i++)
            Texture[i] *= new Vector2(16f / 256f);
    }

    public override void RenderInto(BlockDescriptor descriptor, Vector3 offset, VisibleFaces faces,
        Tuple<int, int> textureMap,
        List<VertexPositionNormalColorTexture> vertices, List<int> indices)
    {
        Span<int> lighting = stackalloc int[6];
        for (var i = 0; i < 6; i++)
            lighting[i] = GetLight(descriptor.Chunk, descriptor.Coordinates + FaceCoords[i]);

        var start = vertices.Count;
        CreateUniformCubeInto(Vector3.Zero, Texture, faces, Color.White, lighting, vertices, indices);
        var heightMultiplier = new Vector3(1, (descriptor.Metadata + 1) / 16f, 1);
        var span = CollectionsMarshal.AsSpan(vertices).Slice(start);
        for (var i = 0; i < span.Length; i++)
        {
            if (span[i].Position.Y > 0)
                span[i].Position *= heightMultiplier;
            span[i].Position += offset;
        }
    }
}
