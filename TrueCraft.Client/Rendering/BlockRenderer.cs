using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.API.World;
using TrueCraft.Core.World;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace TrueCraft.Client.Rendering;

public class BlockRenderer
{
    private static readonly BlockRenderer DefaultRenderer = new BlockRenderer();
    private static readonly BlockRenderer[] Renderers = new BlockRenderer[0x100];

    protected static readonly VisibleFaces[] VisibleForCubeFace =
    {
        VisibleFaces.South,
        VisibleFaces.North,
        VisibleFaces.East,
        VisibleFaces.West,
        VisibleFaces.Top,
        VisibleFaces.Bottom
    };

    protected static readonly Vector3[][] CubeMesh;

    protected static readonly Vector3[] CubeNormals =
    {
        new Vector3(0, 0, 1),
        new Vector3(0, 0, -1),
        new Vector3(1, 0, 0),
        new Vector3(-1, 0, 0),
        new Vector3(0, 1, 0),
        new Vector3(0, -1, 0)
    };

    static BlockRenderer()
    {
        for (var i = 0; i < Renderers.Length; i++) Renderers[i] = DefaultRenderer;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        foreach (var type in assembly.GetTypes().Where(t =>
            typeof(BlockRenderer).IsAssignableFrom(t) && !t.IsAbstract && t != typeof(BlockRenderer)))
            Activator.CreateInstance(type); // This is just to call the static initializers

        CubeMesh = new Vector3[6][];

        CubeMesh[0] = new[] // Positive Z face
        {
            new Vector3(1, 0, 1),
            new Vector3(0, 0, 1),
            new Vector3(0, 1, 1),
            new Vector3(1, 1, 1)
        };

        CubeMesh[1] = new[] // Negative Z face
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(1, 1, 0),
            new Vector3(0, 1, 0)
        };

        CubeMesh[2] = new[] // Positive X face
        {
            new Vector3(1, 0, 0),
            new Vector3(1, 0, 1),
            new Vector3(1, 1, 1),
            new Vector3(1, 1, 0)
        };

        CubeMesh[3] = new[] // Negative X face
        {
            new Vector3(0, 0, 1),
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 1, 1)
        };

        CubeMesh[4] = new[] // Positive Y face
        {
            new Vector3(1, 1, 1),
            new Vector3(0, 1, 1),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0)
        };

        CubeMesh[5] = new[] // Negative Y face
        {
            new Vector3(1, 0, 0),
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 1)
        };
    }

    public static void RegisterRenderer(byte id, BlockRenderer renderer)
    {
        Renderers[id] = renderer;
    }

    // ----- Hot-path entry points (used by ChunkRenderer) -----

    /// <summary>
    ///     Renders the given block by appending its geometry directly into the supplied
    ///     accumulator buffers. No per-block heap allocations.
    /// </summary>
    public static void RenderBlockInto(IBlockProvider provider, BlockDescriptor descriptor,
        VisibleFaces faces, Vector3 offset,
        Buffer<VertexPositionNormalColorTexture> vertices, Buffer<int> indices)
    {
        var textureMap = provider.GetTextureMap(descriptor.Metadata) ?? new Tuple<int, int>(0, 0);
        Renderers[descriptor.ID].RenderInto(descriptor, offset, faces, textureMap, vertices, indices);
    }

    /// <summary>
    ///     Tile size in atlas-normalised UV space (16 px tile / 256 px atlas).
    /// </summary>
    protected const float AtlasTileStride = 16f / 256f;

    /// <summary>
    ///     Appends this block's geometry into <paramref name="vertices"/> and
    ///     <paramref name="indices"/>. The default implementation emits a uniform
    ///     cube using the provider's single texture-map coordinate.
    /// </summary>
    public virtual void RenderInto(BlockDescriptor descriptor, Vector3 offset, VisibleFaces faces,
        Tuple<int, int> textureMap,
        Buffer<VertexPositionNormalColorTexture> vertices, Buffer<int> indices)
    {
        var baseUV = new Vector2(textureMap.Item1 * AtlasTileStride, textureMap.Item2 * AtlasTileStride);

        Span<int> lighting = stackalloc int[6];
        for (var i = 0; i < 6; i++)
            lighting[i] = GetLight(descriptor.Chunk, descriptor.Coordinates + FaceCoords[i]);

        CreateUniformCubeInto(offset, baseUV, AtlasTileStride, faces, Color.White, lighting, vertices, indices);
    }

    // ----- Legacy array-returning wrappers (cold path; used by IconRenderer + HighlightModule) -----

    /// <summary>
    ///     Legacy form that returns freshly-allocated arrays. Retained for callers
    ///     that build a one-off Mesh (such as <c>IconRenderer</c>); the hot path
    ///     in <c>ChunkRenderer</c> uses <see cref="RenderBlockInto"/> instead.
    /// </summary>
    public static VertexPositionNormalColorTexture[] RenderBlock(IBlockProvider provider,
        BlockDescriptor descriptor,
        VisibleFaces faces, Vector3 offset, int indexesOffset, out int[] indexes)
    {
        var verts = new Buffer<VertexPositionNormalColorTexture>(64);
        var idxs = new Buffer<int>(64);
        RenderBlockInto(provider, descriptor, faces, offset, verts, idxs);

        if (indexesOffset != 0)
            for (var i = 0; i < idxs.Count; i++)
                idxs.Array[i] += indexesOffset;

        return DetachToExactSize(verts, idxs, out indexes);
    }

    /// <summary>
    ///     Legacy form retained for external callers like <c>HighlightModule</c>.
    ///     Internally routes through <see cref="CreateUniformCubeInto(Vector3, Vector2[], VisibleFaces, Color, ReadOnlySpan{int}, Buffer{VertexPositionNormalColorTexture}, Buffer{int})"/>.
    /// </summary>
    public static VertexPositionNormalColorTexture[] CreateUniformCube(Vector3 offset, Vector2[] texture,
        VisibleFaces faces, int indexesOffset, out int[] indexes, Color color, int[] lighting = null)
    {
        var verts = new Buffer<VertexPositionNormalColorTexture>(64);
        var idxs = new Buffer<int>(64);
        ReadOnlySpan<int> lightingSpan = lighting ?? DefaultLighting;
        CreateUniformCubeInto(offset, texture, faces, color, lightingSpan, verts, idxs);

        if (indexesOffset != 0)
            for (var i = 0; i < idxs.Count; i++)
                idxs.Array[i] += indexesOffset;

        return DetachToExactSize(verts, idxs, out indexes);
    }

    private static VertexPositionNormalColorTexture[] DetachToExactSize(
        Buffer<VertexPositionNormalColorTexture> verts, Buffer<int> idxs, out int[] indexes)
    {
        var (vArr, vCount) = verts.Detach();
        var (iArr, iCount) = idxs.Detach();
        var vertOut = new VertexPositionNormalColorTexture[vCount];
        indexes = new int[iCount];
        Array.Copy(vArr, vertOut, vCount);
        Array.Copy(iArr, indexes, iCount);
        System.Buffers.ArrayPool<VertexPositionNormalColorTexture>.Shared.Return(vArr);
        System.Buffers.ArrayPool<int>.Shared.Return(iArr);
        return vertOut;
    }

    protected static VertexPositionNormalColorTexture[] CreateQuad(CubeFace face, Vector3 offset,
        Vector2[] texture, int textureOffset, int indiciesOffset, out int[] indicies, Color color)
    {
        var quad = new VertexPositionNormalColorTexture[4];
        indicies = new int[6];
        EmitQuad(face, offset, texture, textureOffset, indiciesOffset, color, quad, 0, indicies, 0);
        return quad;
    }

    // ----- Core emitters (no allocations beyond list growth) -----

    /// <summary>
    ///     Appends a uniform cube into <paramref name="vertexDest"/> /
    ///     <paramref name="indexDest"/>. Walks the 6 cube faces, calling
    ///     <see cref="EmitQuadInto(CubeFace, Vector3, Vector2[], int, Color, List{VertexPositionNormalColorTexture}, List{int})"/>
    ///     for each visible one. Per-face texture coordinates are taken from
    ///     <paramref name="texture"/>, which lays out 4 corners per face × 6
    ///     faces — used by renderers that need distinct textures per face
    ///     (logs, slabs, grass, …).
    /// </summary>
    protected static void CreateUniformCubeInto(Vector3 offset, Vector2[] texture, VisibleFaces faces,
        Color color, ReadOnlySpan<int> lighting,
        Buffer<VertexPositionNormalColorTexture> vertexDest, Buffer<int> indexDest)
    {
        faces = VisibleFaces.All; // Temporary — same as the legacy path.
        if (lighting.IsEmpty)
            lighting = DefaultLighting;

        var textureIndex = 0;
        for (var _side = 0; _side < 6; _side++)
        {
            if ((faces & VisibleForCubeFace[_side]) == 0)
            {
                textureIndex += 4;
                continue;
            }

            var lightColor = LightColor.ToVector3() * CubeBrightness[lighting[_side]];
            var side = (CubeFace) _side;
            EmitQuadInto(side, offset, texture, textureIndex % texture.Length,
                new Color(lightColor * color.ToVector3()), vertexDest, indexDest);
            textureIndex += 4;
        }
    }

    /// <summary>
    ///     Uniform-cube overload that takes a single UV origin + tile stride
    ///     instead of a per-face Vector2[]. All six faces use the same UV
    ///     rectangle. Caller does not need to allocate any per-block array.
    /// </summary>
    protected static void CreateUniformCubeInto(Vector3 offset, Vector2 baseUV, float stride,
        VisibleFaces faces, Color color, ReadOnlySpan<int> lighting,
        Buffer<VertexPositionNormalColorTexture> vertexDest, Buffer<int> indexDest)
    {
        faces = VisibleFaces.All;
        if (lighting.IsEmpty)
            lighting = DefaultLighting;

        for (var _side = 0; _side < 6; _side++)
        {
            if ((faces & VisibleForCubeFace[_side]) == 0)
                continue;

            var lightColor = LightColor.ToVector3() * CubeBrightness[lighting[_side]];
            var side = (CubeFace) _side;
            EmitQuadInto(side, offset, baseUV, stride,
                new Color(lightColor * color.ToVector3()), vertexDest, indexDest);
        }
    }

    /// <summary>
    ///     Appends one cube-face quad to the destination lists. Indices reference
    ///     vertices by absolute position in <paramref name="vertexDest"/> at append
    ///     time; no caller-provided index offset is needed.
    /// </summary>
    protected static void EmitQuadInto(CubeFace face, Vector3 offset, Vector2[] texture,
        int textureOffset, Color color,
        Buffer<VertexPositionNormalColorTexture> vertexDest, Buffer<int> indexDest)
    {
        var faceIndex = (int) face;
        var unit = CubeMesh[faceIndex];
        var normal = CubeNormals[faceIndex];
        var faceColor = new Color(FaceBrightness[faceIndex] * color.ToVector3());

        var baseIdx = vertexDest.Count;
        vertexDest.Add(new VertexPositionNormalColorTexture(
            offset + unit[0], normal, faceColor, texture[textureOffset + 0]));
        vertexDest.Add(new VertexPositionNormalColorTexture(
            offset + unit[1], normal, faceColor, texture[textureOffset + 1]));
        vertexDest.Add(new VertexPositionNormalColorTexture(
            offset + unit[2], normal, faceColor, texture[textureOffset + 2]));
        vertexDest.Add(new VertexPositionNormalColorTexture(
            offset + unit[3], normal, faceColor, texture[textureOffset + 3]));

        indexDest.Add(baseIdx + 0);
        indexDest.Add(baseIdx + 1);
        indexDest.Add(baseIdx + 3);
        indexDest.Add(baseIdx + 1);
        indexDest.Add(baseIdx + 2);
        indexDest.Add(baseIdx + 3);
    }

    /// <summary>
    ///     Per-face emitter for the uniform-cube path: same UV rectangle on
    ///     every face, computed inline from a single origin + stride. Avoids
    ///     the 4-element Vector2[] the array-based overload requires.
    /// </summary>
    protected static void EmitQuadInto(CubeFace face, Vector3 offset, Vector2 baseUV, float stride,
        Color color,
        Buffer<VertexPositionNormalColorTexture> vertexDest, Buffer<int> indexDest)
    {
        var faceIndex = (int) face;
        var unit = CubeMesh[faceIndex];
        var normal = CubeNormals[faceIndex];
        var faceColor = new Color(FaceBrightness[faceIndex] * color.ToVector3());

        var uv0 = new Vector2(baseUV.X + stride, baseUV.Y + stride);
        var uv1 = new Vector2(baseUV.X,          baseUV.Y + stride);
        var uv2 = baseUV;
        var uv3 = new Vector2(baseUV.X + stride, baseUV.Y);

        var baseIdx = vertexDest.Count;
        vertexDest.Add(new VertexPositionNormalColorTexture(offset + unit[0], normal, faceColor, uv0));
        vertexDest.Add(new VertexPositionNormalColorTexture(offset + unit[1], normal, faceColor, uv1));
        vertexDest.Add(new VertexPositionNormalColorTexture(offset + unit[2], normal, faceColor, uv2));
        vertexDest.Add(new VertexPositionNormalColorTexture(offset + unit[3], normal, faceColor, uv3));

        indexDest.Add(baseIdx + 0);
        indexDest.Add(baseIdx + 1);
        indexDest.Add(baseIdx + 3);
        indexDest.Add(baseIdx + 1);
        indexDest.Add(baseIdx + 2);
        indexDest.Add(baseIdx + 3);
    }

    // Legacy array-based EmitQuad used by CreateQuad above; kept as a thin shim.
    private static void EmitQuad(CubeFace face, Vector3 offset, Vector2[] texture,
        int textureOffset, int indicesOffset, Color color,
        VertexPositionNormalColorTexture[] vertexDest, int vertexDestStart,
        int[] indexDest, int indexDestStart)
    {
        var faceIndex = (int) face;
        var baseIndex = faceIndex * 4 + indicesOffset;
        indexDest[indexDestStart + 0] = 0 + baseIndex;
        indexDest[indexDestStart + 1] = 1 + baseIndex;
        indexDest[indexDestStart + 2] = 3 + baseIndex;
        indexDest[indexDestStart + 3] = 1 + baseIndex;
        indexDest[indexDestStart + 4] = 2 + baseIndex;
        indexDest[indexDestStart + 5] = 3 + baseIndex;

        var unit = CubeMesh[faceIndex];
        var normal = CubeNormals[faceIndex];
        var faceColor = new Color(FaceBrightness[faceIndex] * color.ToVector3());

        vertexDest[vertexDestStart + 0] = new VertexPositionNormalColorTexture(
            offset + unit[0], normal, faceColor, texture[textureOffset + 0]);
        vertexDest[vertexDestStart + 1] = new VertexPositionNormalColorTexture(
            offset + unit[1], normal, faceColor, texture[textureOffset + 1]);
        vertexDest[vertexDestStart + 2] = new VertexPositionNormalColorTexture(
            offset + unit[2], normal, faceColor, texture[textureOffset + 2]);
        vertexDest[vertexDestStart + 3] = new VertexPositionNormalColorTexture(
            offset + unit[3], normal, faceColor, texture[textureOffset + 3]);
    }

    protected enum CubeFace
    {
        PositiveZ = 0,
        NegativeZ = 1,
        PositiveX = 2,
        NegativeX = 3,
        PositiveY = 4,
        NegativeY = 5
    }

    #region Lighting

    /// <summary>
    ///     The per-vertex light color to apply to blocks.
    /// </summary>
    protected static readonly Color LightColor =
        new Color(245, 245, 225);

    /// <summary>
    ///     The default lighting information for rendering a block;
    ///     i.e. when the lighting param to CreateUniformCube is null.
    /// </summary>
    protected static readonly int[] DefaultLighting =
    {
        15, 15, 15,
        15, 15, 15
    };

    /// <summary>
    ///     The per-face brightness modifier for lighting.
    /// </summary>
    protected static readonly float[] FaceBrightness =
    {
        0.6f, 0.6f, // North / South
        0.8f, 0.8f, // East / West
        1.0f, 0.5f // Top / Bottom
    };

    /// <summary>
    ///     The offset coordinates used to get the position of a block for a face.
    /// </summary>
    protected static readonly Coordinates3D[] FaceCoords =
    {
        Coordinates3D.South, Coordinates3D.North,
        Coordinates3D.East, Coordinates3D.West,
        Coordinates3D.Up, Coordinates3D.Down
    };

    /// <summary>
    ///     Maps a light level [0..15] to a brightness modifier for lighting.
    /// </summary>
    protected static readonly float[] CubeBrightness =
    {
        0.050f, 0.067f, 0.085f, 0.106f, // [ 0..3 ]
        0.129f, 0.156f, 0.186f, 0.221f, // [ 4..7 ]
        0.261f, 0.309f, 0.367f, 0.437f, // [ 8..11]
        0.525f, 0.638f, 0.789f, 1.000f //  [12..15]
    };

    /// <summary>
    /// </summary>
    /// <param name="chunk"></param>
    /// <param name="coords"></param>
    /// <returns></returns>
    protected static int GetLight(IChunk chunk, Coordinates3D coords)
    {
        // Handle icon renderer.
        if (chunk is null)
            return 15;

        // Handle top (and bottom) of the world.
        if (coords.Y < 0)
            return 0;
        if (coords.Y >= Chunk.Height)
            return 15;

        // Handle coordinates outside the chunk.
        if (coords.X < 0 || coords.X >= Chunk.Width ||
            coords.Z < 0 || coords.Z >= Chunk.Depth)
            return 15;

        return Math.Min(chunk.GetBlockLight(coords) + chunk.GetSkyLight(coords), 15);
    }

    #endregion
}
