using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Client.Rendering.Blocks;

public class WheatRenderer : BlockRenderer
{
    private readonly Vector2[][] Textures;

    static WheatRenderer()
    {
        RegisterRenderer(CropsBlock.BlockID, new WheatRenderer());
    }

    public WheatRenderer()
    {
        var textureMap = new Vector2(8, 5);
        Textures = new Vector2[8][];
        for (var i = 0; i < 8; i++)
        {
            Textures[i] = new[]
            {
                textureMap + Vector2.UnitX + Vector2.UnitY,
                textureMap + Vector2.UnitY,
                textureMap,
                textureMap + Vector2.UnitX
            };
            for (var j = 0; j < Textures[i].Length; j++)
                Textures[i][j] *= new Vector2(16f / 256f);
            textureMap += new Vector2(1, 0);
        }
    }

    public override void RenderInto(BlockDescriptor descriptor, Vector3 offset, VisibleFaces faces,
        Tuple<int, int> textureMap,
        Buffer<VertexPositionNormalColorTexture> vertices, Buffer<int> indices)
    {
        // Wheat is rendered by rendering the four vertical faces of a cube, then moving
        // them towards the middle. A second set of four faces is rendered with reversed
        // X/Z so each face is visible from the opposite side (avoids culling).
        //
        // The legacy path allocated 48 vertex slots but only filled 32; the trailing
        // 16 zero-vertices were dead weight. The new path emits exactly 32.
        var texture = Textures[0];
        if (descriptor.Metadata < Textures.Length)
            texture = Textures[descriptor.Metadata];

        var center = new Vector3(-0.5f, -0.5f, -0.5f);
        var start = vertices.Count;

        // First pass: four vertical faces, scaled inward.
        for (var _side = 0; _side < 4; _side++)
        {
            var faceStart = vertices.Count;
            var side = (CubeFace) _side;
            EmitQuadInto(side, center, texture, 0, Color.White, vertices, indices);
            var span = vertices.Array.AsSpan(faceStart, vertices.Count - faceStart);
            if (side == CubeFace.NegativeX || side == CubeFace.PositiveX)
                for (var i = 0; i < span.Length; i++)
                {
                    span[i].Position.X *= 0.5f;
                    span[i].Position += offset;
                }
            else
                for (var i = 0; i < span.Length; i++)
                {
                    span[i].Position.Z *= 0.5f;
                    span[i].Position += offset;
                }
        }

        // Second pass: same four faces with reversed X/Z so the back side is visible.
        for (var _side = 0; _side < 4; _side++)
        {
            var faceStart = vertices.Count;
            var side = (CubeFace) _side;
            EmitQuadInto(side, center, texture, 0, Color.White, vertices, indices);
            var span = vertices.Array.AsSpan(faceStart, vertices.Count - faceStart);
            if (side == CubeFace.NegativeX || side == CubeFace.PositiveX)
                for (var i = 0; i < span.Length; i++)
                {
                    span[i].Position.X *= 0.5f;
                    span[i].Position.X = -span[i].Position.X;
                    span[i].Position += offset;
                }
            else
                for (var i = 0; i < span.Length; i++)
                {
                    span[i].Position.Z *= 0.5f;
                    span[i].Position.Z = -span[i].Position.Z;
                    span[i].Position += offset;
                }
        }

        // Final pass: subtract center and shift Y down by one texel.
        var all = vertices.Array.AsSpan(start, vertices.Count - start);
        for (var i = 0; i < all.Length; i++)
        {
            all[i].Position.Y -= 1 / 16f;
            all[i].Position -= center;
        }
    }
}
