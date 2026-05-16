using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Client.Rendering.Blocks;

public class CactusRenderer : BlockRenderer
{
    private static readonly Vector2 TextureMap = new Vector2(6, 4);

    private static readonly Vector2[] Texture =
    {
        TextureMap + Vector2.UnitX + Vector2.UnitY,
        TextureMap + Vector2.UnitY,
        TextureMap,
        TextureMap + Vector2.UnitX
    };

    private static readonly Vector2 TopTextureMap = new Vector2(6, 4);

    private static readonly Vector2[] TopTexture =
    {
        TopTextureMap + Vector2.UnitX + Vector2.UnitY,
        TopTextureMap + Vector2.UnitY,
        TopTextureMap,
        TopTextureMap + Vector2.UnitX
    };

    static CactusRenderer()
    {
        RegisterRenderer(CactusBlock.BlockID, new CactusRenderer());
        for (var j = 0; j < Texture.Length; j++)
            Texture[j] *= new Vector2(16f / 256f);
        for (var j = 0; j < TopTexture.Length; j++)
            TopTexture[j] *= new Vector2(16f / 256f);
    }

    public override void RenderInto(BlockDescriptor descriptor, Vector3 offset, VisibleFaces faces,
        Tuple<int, int> textureMap,
        List<VertexPositionNormalColorTexture> vertices, List<int> indices)
    {
        // This is similar to how wheat is rendered.
        //
        // The legacy path allocated `new VertexPositionNormalColorTexture[5 * 6]` but
        // only ever wrote 5 * 4 = 20 vertices into it, leaving 10 zero-initialized
        // vertices trailing in the buffer that no index referenced. The new path
        // emits exactly 20 vertices — strictly less GPU memory per cactus block.
        var center = new Vector3(-0.5f, -0.5f, -0.5f);
        var start = vertices.Count;
        for (var _side = 0; _side < 4; _side++)
        {
            var faceStart = vertices.Count;
            var side = (CubeFace) _side;
            EmitQuadInto(side, center, Texture, 0, Color.White, vertices, indices);
            var span = CollectionsMarshal.AsSpan(vertices).Slice(faceStart);
            if (side == CubeFace.NegativeX || side == CubeFace.PositiveX)
                for (var i = 0; i < span.Length; i++)
                {
                    span[i].Position.X *= 14f / 16f;
                    span[i].Position += offset;
                }
            else
                for (var i = 0; i < span.Length; i++)
                {
                    span[i].Position.Z *= 14f / 16f;
                    span[i].Position += offset;
                }
        }

        // Top face — same Z-scale path as the non-X sides above (the legacy code's
        // X/Z branch was effectively dead for PositiveY).
        {
            var faceStart = vertices.Count;
            EmitQuadInto(CubeFace.PositiveY, center, TopTexture, 0, Color.White, vertices, indices);
            var span = CollectionsMarshal.AsSpan(vertices).Slice(faceStart);
            for (var i = 0; i < span.Length; i++)
            {
                span[i].Position.Z *= 14f / 16f;
                span[i].Position += offset;
            }
        }

        // Final pass: subtract center and shift Y down by one texel.
        var all = CollectionsMarshal.AsSpan(vertices).Slice(start);
        for (var i = 0; i < all.Length; i++)
        {
            all[i].Position.Y -= 1 / 16f;
            all[i].Position -= center;
        }
    }
}
