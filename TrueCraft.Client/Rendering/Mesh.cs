using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TrueCraft.Client.Rendering
{
    /// <summary>
    ///     Represents an indexed collection of data that can be rendered.
    /// </summary>
    public class Mesh : IDisposable
    {
        /// <summary>
        ///     The maximum number of submeshes stored in a single mesh.
        /// </summary>
        public const int SubmeshLimit = 16;

        // Used for synchronous access to the graphics device.
        private static readonly object SyncLock =
            new object();

        private readonly TrueCraftGame _game;
        private GraphicsDevice _graphicsDevice;
        protected IndexBuffer[] _indices;

        private readonly bool _recalculateBounds; // Whether this mesh should recalculate its bounding box when changed.

        protected VertexBuffer
            _vertices; // ChunkMesh uses these but external classes shouldn't, so I've made them protected.

        /// <summary>
        ///     Creates a new mesh.
        /// </summary>
        public Mesh(TrueCraftGame game, int submeshes = 1, bool recalculateBounds = true)
        {
            if (submeshes < 0 || submeshes >= SubmeshLimit)
                throw new ArgumentOutOfRangeException();

            _game = game;
            _graphicsDevice = game.GraphicsDevice;
            _indices = new IndexBuffer[submeshes];
            _recalculateBounds = recalculateBounds;
        }

        /// <summary>
        ///     Creates a new mesh.
        /// </summary>
        public Mesh(TrueCraftGame game, VertexPositionNormalColorTexture[] vertices,
            int submeshes = 1, bool recalculateBounds = true) : this(game, submeshes, recalculateBounds)
        {
            Vertices = vertices;
        }

        /// <summary>
        ///     Creates a new mesh.
        /// </summary>
        public Mesh(TrueCraftGame game, VertexPositionNormalColorTexture[] vertices,
            int[] indices, bool recalculateBounds = true) : this(game, 1, recalculateBounds)
        {
            Vertices = vertices;
            SetSubmesh(0, indices);
        }

        public static int VerticiesRendered { get; set; }
        public static int IndiciesRendered { get; set; }

        /// <summary>
        ///     Gets or sets the vertices in this mesh.
        /// </summary>
        public VertexPositionNormalColorTexture[] Vertices
        {
            set
            {
                _vertices?.Dispose();

                _game.Invoke(() =>
                {
                    _vertices = new VertexBuffer(_graphicsDevice, VertexPositionNormalColorTexture.VertexDeclaration,
                        value.Length, BufferUsage.WriteOnly);
                    _vertices.SetData(value);
                    IsReady = true;
                });

                if (_recalculateBounds)
                    BoundingBox = RecalculateBounds(value);
            }
        }

        public bool IsReady { get; private set; }

        public int Submeshes { get; private set; }

        /// <summary>
        ///     Gets the bounding box for this mesh.
        /// </summary>
        public BoundingBox BoundingBox { get; private set; }

        private bool _disposed;

        /// <summary>
        ///     Gets whether this mesh is disposed of.
        /// </summary>
        public bool IsDisposed => _disposed;

        /// <summary>
        ///     Disposes of this mesh.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static void ResetStats()
        {
            VerticiesRendered = 0;
            IndiciesRendered = 0;
        }

        /// <summary>
        ///     Sets a submesh in this mesh.
        /// </summary>
        public void SetSubmesh(int index, int[] indices)
        {
            if (index < 0 || index > _indices.Length)
                throw new ArgumentOutOfRangeException();

            lock (SyncLock)
            {
                if (_indices[index] is not null)
                    _indices[index].Dispose();

                _game.Invoke(() =>
                {
                    _indices[index] = new IndexBuffer(_graphicsDevice, typeof(int),
                        indices.Length, BufferUsage.WriteOnly);
                    _indices[index].SetData(indices);
                    if (index + 1 > Submeshes)
                        Submeshes = index + 1;
                });
            }
        }

        /// <summary>
        ///     Draws this mesh using the specified effect.
        /// </summary>
        /// <param name="effect">The effect to use.</param>
        public void Draw(Effect effect)
        {
            if (effect is null)
                throw new ArgumentException();

            for (var i = 0; i < _indices.Length; i++)
                Draw(effect, i);
        }

        /// <summary>
        ///     Draws a submesh in this mesh using the specified effect.
        /// </summary>
        /// <param name="effect">The effect to use.</param>
        /// <param name="index">The submesh index.</param>
        public void Draw(Effect effect, int index)
        {
            if (effect is null)
                throw new ArgumentException();

            if (index < 0 || index > _indices.Length)
                throw new ArgumentOutOfRangeException();

            if (_vertices is null || _vertices.IsDisposed || _indices[index] is null || _indices[index].IsDisposed ||
                _indices[index].IndexCount < 3)
                return; // Invalid state for rendering, just return.

            effect.GraphicsDevice.SetVertexBuffer(_vertices);
            effect.GraphicsDevice.Indices = _indices[index];
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                // deprecated
                // effect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _indices[index].IndexCount, 0, _indices[index].IndexCount / 3);

                effect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                    _indices[index].IndexCount / 3);
            }

            VerticiesRendered += _vertices.VertexCount;
            IndiciesRendered += _indices[index].IndexCount;
        }

        /// <summary>
        ///     Returns the total vertices in this mesh.
        /// </summary>
        public int GetTotalVertices()
        {
            if (_vertices is null)
                return 0;

            lock (SyncLock)
            {
                return _vertices.VertexCount;
            }
        }

        /// <summary>
        ///     Returns the total indices for all the submeshes in this mesh.
        /// </summary>
        public int GetTotalIndices()
        {
            lock (SyncLock)
            {
                var sum = 0;
                foreach (var element in _indices)
                    sum += element?.IndexCount ?? 0;
                return sum;
            }
        }

        /// <summary>
        ///     Recalculates the bounding box for this mesh.
        /// </summary>
        /// <param name="vertices">The vertices in this mesh.</param>
        /// <returns></returns>
        protected virtual BoundingBox RecalculateBounds(VertexPositionNormalColorTexture[] vertices)
        {
            return ComputeAxisAlignedBounds(vertices);
        }

        internal static BoundingBox ComputeAxisAlignedBounds(VertexPositionNormalColorTexture[] vertices)
        {
            if (vertices is null || vertices.Length == 0)
                return default;

            var first = vertices[0].Position;
            float minX = first.X, minY = first.Y, minZ = first.Z;
            float maxX = first.X, maxY = first.Y, maxZ = first.Z;

            for (var i = 1; i < vertices.Length; i++)
            {
                var p = vertices[i].Position;
                if (p.X < minX) minX = p.X; else if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y; else if (p.Y > maxY) maxY = p.Y;
                if (p.Z < minZ) minZ = p.Z; else if (p.Z > maxZ) maxZ = p.Z;
            }

            return new BoundingBox(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
        }

        /// <summary>
        ///     Disposes of this mesh.
        /// </summary>
        /// <param name="disposing">Whether Dispose() called the method.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _graphicsDevice = null; // Lose the reference to our graphics device.

                _vertices?.Dispose();
                foreach (var element in _indices) element?.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        ///     Finalizes this mesh.
        /// </summary>
        ~Mesh()
        {
            Dispose(false);
        }
    }
}