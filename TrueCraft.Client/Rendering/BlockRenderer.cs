﻿using System;
using System.Linq;
using Microsoft.Xna.Framework;
using TrueCraft.API.Logic;
using TrueCraft.API.World;
using TrueCraft.Core.World;
using Coordinates3D = TrueCraft.API.Coordinates3D;

namespace TrueCraft.Client.Rendering
{
    public class BlockRenderer
    {
        private static BlockRenderer DefaultRenderer = new BlockRenderer();
        private static BlockRenderer[] Renderers = new BlockRenderer[0x100];

        public static void RegisterRenderer(byte id, BlockRenderer renderer)
        {
            Renderers[id] = renderer;
        }

        public static VertexPositionNormalColorTexture[] RenderBlock(IBlockProvider provider, BlockDescriptor descriptor,
            VisibleFaces faces, Vector3 offset, int indexesOffset, out int[] indexes)
        {
            var textureMap = provider.GetTextureMap(descriptor.Metadata) ?? new Tuple<int, int>(0, 0);
            return Renderers[descriptor.ID].Render(descriptor, offset, faces, textureMap, indexesOffset, out indexes);
        }

        public virtual VertexPositionNormalColorTexture[] Render(BlockDescriptor descriptor, Vector3 offset,
            VisibleFaces faces, Tuple<int, int> textureMap, int indiciesOffset, out int[] indicies)
        {
            var texCoords = new Vector2(textureMap.Item1, textureMap.Item2);
            var texture = new[]
            {
                texCoords + Vector2.UnitX + Vector2.UnitY,
                texCoords + Vector2.UnitY,
                texCoords,
                texCoords + Vector2.UnitX
            };

            for (int i = 0; i < texture.Length; i++)
                texture[i] *= new Vector2(16f / 256f);

            var lighting = new int[6];
            for (int i = 0; i < 6; i++)
            {
                var coords = (descriptor.Coordinates + FaceCoords[i]);
                lighting[i] = GetLight(descriptor.Chunk, coords);
            }

            return CreateUniformCube(offset, texture, faces, indiciesOffset, out indicies, Color.White, lighting);
        }

        public static VertexPositionNormalColorTexture[] CreateUniformCube(Vector3 offset, Vector2[] texture,
            VisibleFaces faces, int indexesOffset, out int[] indexes, Color color, int[] lighting = null)
        {
            faces = VisibleFaces.All; // Temporary
            if (lighting == null)
                lighting = DefaultLighting;

            int totalFaces = 0;
            uint f = (uint)faces;
            while (f != 0)
            {
                if ((f & 1) == 1)
                    totalFaces++;
                f >>= 1;
            }

            indexes = new int[6 * totalFaces];
            var vertexes = new VertexPositionNormalColorTexture[4 * totalFaces];
            int textureIndex = 0;
            int sidesSoFar = 0;
            for (int _side = 0; _side < 6; _side++)
            {
                if ((faces & VisibleForCubeFace[_side]) == 0)
                {
                    textureIndex += 4;
                    continue;
                }
                var lightColor = LightColor.ToVector3() * CubeBrightness[lighting[_side]];

                var side = (CubeFace)_side;
                var quad = CreateQuad(side, offset, texture, textureIndex % texture.Length, indexesOffset,
                    out var _indexes, new Color(lightColor * color.ToVector3()));
                Array.Copy(quad, 0, vertexes, sidesSoFar * 4, 4);
                Array.Copy(_indexes, 0, indexes, sidesSoFar * 6, 6);
                textureIndex += 4;
                sidesSoFar++;
            }
            return vertexes;
        }

        protected static VertexPositionNormalColorTexture[] CreateQuad(CubeFace face, Vector3 offset,
            Vector2[] texture, int textureOffset, int indiciesOffset, out int[] indicies, Color color)
        {
            indicies = new[] { 0, 1, 3, 1, 2, 3 };
            for (int i = 0; i < indicies.Length; i++)
                indicies[i] += ((int)face * 4) + indiciesOffset;
            var quad = new VertexPositionNormalColorTexture[4];
            var unit = CubeMesh[(int)face];
            var normal = CubeNormals[(int)face];
            var faceColor = new Color(FaceBrightness[(int)face] * color.ToVector3());
            for (int i = 0; i < 4; i++)
            {
                quad[i] = new VertexPositionNormalColorTexture(offset + unit[i], normal, faceColor, texture[textureOffset + i]);
            }
            return quad;
        }

        #region Lighting

        /// <summary>
        /// The per-vertex light color to apply to blocks.
        /// </summary>
        protected static readonly Color LightColor =
            new Color(245, 245, 225);

        /// <summary>
        /// The default lighting information for rendering a block;
        ///  i.e. when the lighting param to CreateUniformCube == null.
        /// </summary>
        protected static readonly int[] DefaultLighting =
            new int[]
            {
                15, 15, 15,
                15, 15, 15
            };

        /// <summary>
        /// The per-face brightness modifier for lighting.
        /// </summary>
        protected static readonly float[] FaceBrightness =
            new float[]
            {
                0.6f, 0.6f, // North / South
                0.8f, 0.8f, // East / West
                1.0f, 0.5f  // Top / Bottom
            };
        
        /// <summary>
        /// The offset coordinates used to get the position of a block for a face.
        /// </summary>
        protected static readonly Coordinates3D[] FaceCoords =
            {
                Coordinates3D.South, Coordinates3D.North,
                Coordinates3D.East, Coordinates3D.West,
                Coordinates3D.Up, Coordinates3D.Down
            };

        /// <summary>
        /// Maps a light level [0..15] to a brightness modifier for lighting.
        /// </summary>
        protected static readonly float[] CubeBrightness =
            new float[]
            {
                0.050f, 0.067f, 0.085f, 0.106f, // [ 0..3 ]
                0.129f, 0.156f, 0.186f, 0.221f, // [ 4..7 ]
                0.261f, 0.309f, 0.367f, 0.437f, // [ 8..11]
                0.525f, 0.638f, 0.789f, 1.000f //  [12..15]
            };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="coords"></param>
        /// <returns></returns>
        protected static int GetLight(IChunk chunk, Coordinates3D coords)
        {
            // Handle icon renderer.
            if (chunk == null)
                return 15;

            // Handle top (and bottom) of the world.
            if (coords.Y < 0)
                return 0;
            if (coords.Y >= Chunk.Height)
                return 15;

            // Handle coordinates outside the chunk.
            if ((coords.X < 0) || (coords.X >= Chunk.Width) ||
                (coords.Z < 0) || (coords.Z >= Chunk.Depth))
            {
                return 15;
            }

            return Math.Min(chunk.GetBlockLight(coords) + chunk.GetSkyLight(coords), 15);
        }

        #endregion

        protected enum CubeFace
        {
            PositiveZ = 0,
            NegativeZ = 1,
            PositiveX = 2,
            NegativeX = 3,
            PositiveY = 4,
            NegativeY = 5
        }

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
            for (int i = 0; i < Renderers.Length; i++)
            {
                Renderers[i] = DefaultRenderer;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes().Where(t =>
                    typeof(BlockRenderer).IsAssignableFrom(t) && !t.IsAbstract && t != typeof(BlockRenderer)))
                {
                    Activator.CreateInstance(type); // This is just to call the static initializers
                }
            }

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
    }
}
