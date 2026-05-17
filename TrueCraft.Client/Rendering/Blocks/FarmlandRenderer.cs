using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Client.Rendering.Blocks;

public class FarmlandRenderer : BlockRenderer
{
    private static readonly Vector2 DryTopTexture = new Vector2(7, 5);
    private static readonly Vector2 MoistTopTexture = new Vector2(6, 5);
    private static readonly Vector2 SideTexture = new Vector2(2, 0);

    private static readonly Vector2[] DryTexture =
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
        DryTopTexture + Vector2.UnitX + Vector2.UnitY,
        DryTopTexture + Vector2.UnitY,
        DryTopTexture,
        DryTopTexture + Vector2.UnitX,
        // Negative Y
        SideTexture + Vector2.UnitX + Vector2.UnitY,
        SideTexture + Vector2.UnitY,
        SideTexture,
        SideTexture + Vector2.UnitX
    };

    private static readonly Vector2[] MoistTexture =
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
        MoistTopTexture + Vector2.UnitX + Vector2.UnitY,
        MoistTopTexture + Vector2.UnitY,
        MoistTopTexture,
        MoistTopTexture + Vector2.UnitX,
        // Negative Y
        SideTexture + Vector2.UnitX + Vector2.UnitY,
        SideTexture + Vector2.UnitY,
        SideTexture,
        SideTexture + Vector2.UnitX
    };

    static FarmlandRenderer()
    {
        RegisterRenderer(FarmlandBlock.BlockID, new FarmlandRenderer());
        for (var i = 0; i < DryTexture.Length; i++)
        {
            DryTexture[i] *= new Vector2(16f / 256f);
            MoistTexture[i] *= new Vector2(16f / 256f);
        }
    }

    public override void RenderInto(BlockDescriptor descriptor, Vector3 offset, VisibleFaces faces,
        Tuple<int, int> textureMap,
        Buffer<VertexPositionNormalColorTexture> vertices, Buffer<int> indices)
    {
        var texture = DryTexture;
        if (descriptor.Metadata == (byte) FarmlandBlock.MoistureLevel.Moist)
            texture = MoistTexture;

        Span<int> lighting = stackalloc int[6];
        for (var i = 0; i < 6; i++)
            lighting[i] = GetLight(descriptor.Chunk, descriptor.Coordinates + FaceCoords[i]);

        var overhead = new Vector3(0.5f, 0.5f, 0.5f);
        var start = vertices.Count;
        CreateUniformCubeInto(overhead, texture, faces, Color.White, lighting, vertices, indices);
        var span = vertices.Array.AsSpan(start, vertices.Count - start);
        for (var i = 0; i < span.Length; i++)
        {
            if (span[i].Position.Y > 0) span[i].Position.Y *= 15f / 16f;
            span[i].Position += offset;
            span[i].Position -= overhead;
        }
    }
}
