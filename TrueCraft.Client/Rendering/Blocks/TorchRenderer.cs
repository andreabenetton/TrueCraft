using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Client.Rendering.Blocks
{
    public class TorchRenderer : BlockRenderer
    {
        private static readonly Vector2
            TextureMap = new Vector2(7, 86); // Note: this is in pixels (torch texture is not a full block)

        private static readonly Vector2[] Texture =
        {
            // Positive Z
            TextureMap + new Vector2(2, 10),
            TextureMap + new Vector2(0, 10),
            TextureMap,
            TextureMap + new Vector2(2, 0),
            // Negative Z
            TextureMap + new Vector2(2, 10),
            TextureMap + new Vector2(0, 10),
            TextureMap,
            TextureMap + new Vector2(2, 0),
            // Positive X
            TextureMap + new Vector2(2, 10),
            TextureMap + new Vector2(0, 10),
            TextureMap,
            TextureMap + new Vector2(2, 0),
            // Negative X
            TextureMap + new Vector2(2, 10),
            TextureMap + new Vector2(0, 10),
            TextureMap,
            TextureMap + new Vector2(2, 0),
            // Positive Y
            TextureMap + new Vector2(2, 2),
            TextureMap + new Vector2(0, 2),
            TextureMap + new Vector2(0, 0),
            TextureMap + new Vector2(2, 0),
            // Negative Y
            TextureMap + new Vector2(2, 4),
            TextureMap + new Vector2(0, 4),
            TextureMap + new Vector2(0, 2),
            TextureMap + new Vector2(2, 2)
        };

        static TorchRenderer()
        {
            RegisterRenderer(TorchBlock.BlockID, new TorchRenderer());
            for (var i = 0; i < Texture.Length; i++)
                Texture[i] /= 256f;
        }

        public override void RenderInto(BlockDescriptor descriptor, Vector3 offset, VisibleFaces faces,
            Tuple<int, int> textureMap,
            List<VertexPositionNormalColorTexture> vertices, List<int> indices)
        {
            Span<int> lighting = stackalloc int[6];
            for (var i = 0; i < 6; i++)
                lighting[i] = GetLight(descriptor.Chunk, descriptor.Coordinates + FaceCoords[i]);

            var centerized = new Vector3(7f / 16f, 0, 7f / 16f);
            var start = vertices.Count;
            CreateUniformCubeInto(Vector3.Zero, Texture, VisibleFaces.All, Color.White, lighting, vertices, indices);
            var span = CollectionsMarshal.AsSpan(vertices).Slice(start);
            for (var i = 0; i < span.Length; i++)
            {
                span[i].Position.X *= 1f / 8f;
                span[i].Position.Z *= 1f / 8f;
                if (span[i].Position.Y > 0)
                    span[i].Position.Y *= 5f / 8f;
                switch ((TorchBlock.TorchDirection) descriptor.Metadata)
                {
                    case TorchBlock.TorchDirection.West:
                        if (span[i].Position.Y == 0)
                            span[i].Position.X += 8f / 16f;
                        else
                            span[i].Position.X += 3f / 16f;
                        span[i].Position.Y += 5f / 16f;
                        break;
                    case TorchBlock.TorchDirection.East:
                        if (span[i].Position.Y == 0)
                            span[i].Position.X -= 8f / 16f;
                        else
                            span[i].Position.X -= 3f / 16f;
                        span[i].Position.Y += 5f / 16f;
                        break;
                    case TorchBlock.TorchDirection.North:
                        if (span[i].Position.Y == 0)
                            span[i].Position.Z += 8f / 16f;
                        else
                            span[i].Position.Z += 3f / 16f;
                        span[i].Position.Y += 5f / 16f;
                        break;
                    case TorchBlock.TorchDirection.South:
                        if (span[i].Position.Y == 0)
                            span[i].Position.Z -= 8f / 16f;
                        else
                            span[i].Position.Z -= 3f / 16f;
                        span[i].Position.Y += 5f / 16f;
                        break;
                    case TorchBlock.TorchDirection.Ground:
                    default:
                        // nop
                        break;
                }

                span[i].Position += offset;
                span[i].Position += centerized;
            }
        }
    }
}