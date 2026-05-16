using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Client.Rendering.Blocks;

public class LadderRenderer : BlockRenderer
{
    private static readonly Vector2 TextureMap = new Vector2(3, 5);

    private static readonly Vector2[] Texture =
    {
        TextureMap + Vector2.UnitX + Vector2.UnitY,
        TextureMap + Vector2.UnitY,
        TextureMap,
        TextureMap + Vector2.UnitX
    };

    static LadderRenderer()
    {
        RegisterRenderer(LadderBlock.BlockID, new LadderRenderer());
        for (var i = 0; i < Texture.Length; i++)
            Texture[i] *= new Vector2(16f / 256f);
    }

    public override void RenderInto(BlockDescriptor descriptor, Vector3 offset, VisibleFaces faces,
        Tuple<int, int> textureMap,
        List<VertexPositionNormalColorTexture> vertices, List<int> indices)
    {
        // EmitQuadInto's indices are derived from vertices.Count at emit time, so the
        // legacy "subtract faceCorrection to bring indices into [0..6)" step from the
        // array-returning path is no longer needed — indices are already correct.
        Vector3 correction;
        var start = vertices.Count;
        switch ((LadderBlock.LadderDirection) descriptor.Metadata)
        {
            case LadderBlock.LadderDirection.North:
                EmitQuadInto(CubeFace.PositiveZ, offset, Texture, 0, Color.White, vertices, indices);
                correction = Vector3.Forward;
                break;
            case LadderBlock.LadderDirection.South:
                EmitQuadInto(CubeFace.NegativeZ, offset, Texture, 0, Color.White, vertices, indices);
                correction = Vector3.Backward;
                break;
            case LadderBlock.LadderDirection.East:
                EmitQuadInto(CubeFace.NegativeX, offset, Texture, 0, Color.White, vertices, indices);
                correction = Vector3.Right;
                break;
            case LadderBlock.LadderDirection.West:
                EmitQuadInto(CubeFace.PositiveX, offset, Texture, 0, Color.White, vertices, indices);
                correction = Vector3.Left;
                break;
            default:
                // Should never happen — fall back to a full cube with default lighting.
                ReadOnlySpan<int> defaultLighting = DefaultLighting;
                CreateUniformCubeInto(offset, Texture, VisibleFaces.All,
                    Color.White, defaultLighting, vertices, indices);
                correction = Vector3.Zero;
                break;
        }

        var span = CollectionsMarshal.AsSpan(vertices).Slice(start);
        for (var i = 0; i < span.Length; i++)
            span[i].Position += correction;
    }
}
