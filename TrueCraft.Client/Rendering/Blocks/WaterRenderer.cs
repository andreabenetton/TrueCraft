using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Client.Rendering.Blocks;

public class WaterRenderer : BlockRenderer
{
    private static readonly Vector2 TextureMap = new Vector2(13, 12);

    private static readonly Vector2[] Texture =
    {
        TextureMap + Vector2.UnitX + Vector2.UnitY,
        TextureMap + Vector2.UnitY,
        TextureMap,
        TextureMap + Vector2.UnitX
    };

    static WaterRenderer()
    {
        RegisterRenderer(WaterBlock.BlockID, new WaterRenderer());
        RegisterRenderer(StationaryWaterBlock.BlockID, new WaterRenderer());
        for (var i = 0; i < Texture.Length; i++)
            Texture[i] *= new Vector2(16f / 256f);
    }

    public override void RenderInto(BlockDescriptor descriptor, Vector3 offset, VisibleFaces faces,
        Tuple<int, int> textureMap,
        Buffer<VertexPositionNormalColorTexture> vertices, Buffer<int> indices)
    {
        Span<int> lighting = stackalloc int[6];
        for (var i = 0; i < 6; i++)
            lighting[i] = GetLight(descriptor.Chunk, descriptor.Coordinates + FaceCoords[i]);

        // TODO: Rest of water rendering (shape and level and so on)
        var overhead = new Vector3(0.5f, 0.5f, 0.5f);
        var start = vertices.Count;
        CreateUniformCubeInto(overhead, Texture, faces, Color.Blue, lighting, vertices, indices);
        var span = vertices.Array.AsSpan(start, vertices.Count - start);
        for (var i = 0; i < span.Length; i++)
        {
            if (span[i].Position.Y > 0) span[i].Position.Y *= 14f / 16f;
            span[i].Position += offset;
            span[i].Position -= overhead;
        }
    }
}
