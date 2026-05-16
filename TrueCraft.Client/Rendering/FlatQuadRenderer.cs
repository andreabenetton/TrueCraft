using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TrueCraft.API.Logic;

namespace TrueCraft.Client.Rendering;

public abstract class FlatQuadRenderer : BlockRenderer
{
    protected static readonly Vector3[] QuadNormals =
    {
        new Vector3(0, 0, 1),
        new Vector3(0, 0, -1),
        new Vector3(1, 0, 0),
        new Vector3(-1, 0, 0),
        new Vector3(0, 1, 0),
        new Vector3(0, -1, 0)
    };

    protected static readonly Vector3[][] QuadMesh;
    protected Vector2[] Texture;

    static FlatQuadRenderer()
    {
        QuadMesh = new Vector3[4][];

        QuadMesh[0] = new[]
        {
            new Vector3(1, 0, 1),
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 1)
        };

        QuadMesh[1] = new[]
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 1),
            new Vector3(1, 1, 1),
            new Vector3(0, 1, 0)
        };

        QuadMesh[2] = new[]
        {
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 0),
            new Vector3(1, 1, 0),
            new Vector3(0, 1, 1)
        };

        QuadMesh[3] = new[]
        {
            new Vector3(1, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, 1, 1),
            new Vector3(1, 1, 0)
        };
    }

    protected FlatQuadRenderer()
    {
        Texture = new[]
        {
            TextureMap + Vector2.UnitX + Vector2.UnitY,
            TextureMap + Vector2.UnitY,
            TextureMap,
            TextureMap + Vector2.UnitX
        };
        for (var i = 0; i < Texture.Length; i++)
            Texture[i] *= new Vector2(16f / 256f);
    }

    protected virtual Vector2 TextureMap => Vector2.Zero;

    public override void RenderInto(BlockDescriptor descriptor, Vector3 offset, VisibleFaces faces,
        Tuple<int, int> textureMap,
        List<VertexPositionNormalColorTexture> vertices, List<int> indices)
    {
        RenderQuadsInto(descriptor, offset, Texture, Color.White, vertices, indices);
    }

    /// <summary>
    ///     Appends four angled quads (cross pattern, used for cobweb / sugarcane /
    ///     vegetation) directly into the destination lists.
    /// </summary>
    protected static void RenderQuadsInto(BlockDescriptor descriptor, Vector3 offset,
        Vector2[] textureMap, Color color,
        List<VertexPositionNormalColorTexture> vertexDest, List<int> indexDest)
    {
        var textureIndex = 0;
        for (var face = 0; face < 4; face++)
        {
            EmitAngledQuadInto(face, offset, textureMap, textureIndex % textureMap.Length,
                color, vertexDest, indexDest);
            textureIndex += 4;
        }
    }

    /// <summary>
    ///     Appends one cross-pattern quad to the destination lists. Indices reference
    ///     vertices by absolute position in <paramref name="vertexDest"/> at append time.
    /// </summary>
    protected static void EmitAngledQuadInto(int face, Vector3 offset, Vector2[] texture,
        int textureOffset, Color color,
        List<VertexPositionNormalColorTexture> vertexDest, List<int> indexDest)
    {
        var unit = QuadMesh[face];
        var normal = CubeNormals[face];

        var baseIdx = vertexDest.Count;
        vertexDest.Add(new VertexPositionNormalColorTexture(
            offset + unit[0], normal, color, texture[textureOffset + 0]));
        vertexDest.Add(new VertexPositionNormalColorTexture(
            offset + unit[1], normal, color, texture[textureOffset + 1]));
        vertexDest.Add(new VertexPositionNormalColorTexture(
            offset + unit[2], normal, color, texture[textureOffset + 2]));
        vertexDest.Add(new VertexPositionNormalColorTexture(
            offset + unit[3], normal, color, texture[textureOffset + 3]));

        indexDest.Add(baseIdx + 0);
        indexDest.Add(baseIdx + 1);
        indexDest.Add(baseIdx + 3);
        indexDest.Add(baseIdx + 1);
        indexDest.Add(baseIdx + 2);
        indexDest.Add(baseIdx + 3);
    }
}
