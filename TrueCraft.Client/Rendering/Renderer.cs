using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TrueCraft.Client.Rendering
{
    /// <summary>
    /// Abstract base class for renderers of meshes.
    /// </summary>
    /// <typeparam name="T">The object to render into a mesh.</typeparam>
    public abstract class Renderer<T> : IDisposable
    {
        private readonly object _syncLock =
            new object();

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<RendererEventArgs<T>> MeshCompleted;

        private volatile bool _isRunning;
        private Thread[] _rendererThreads;
        private volatile bool _isDisposed;
        protected ConcurrentQueue<T> Items, PriorityItems;
        private HashSet<T> _pending;

        /// <summary>
        /// Gets whether this renderer is running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().Name);
                return _isRunning;
            }
        }

        /// <summary>
        /// Gets whether this renderer is disposed of.
        /// </summary>
        public bool IsDisposed => _isDisposed;

        /// <summary>
        /// 
        /// </summary>
        protected Renderer()
        {
            lock (_syncLock)
            {
                _isRunning = false;
                var threads = Environment.ProcessorCount - 2;
                if (threads < 1)
                    threads = 1;
                _rendererThreads = new Thread[threads];
                for (int i = 0; i < _rendererThreads.Length; i++)
                {
                    _rendererThreads[i] = new Thread(DoRendering) { IsBackground = true };
                }
                Items = new ConcurrentQueue<T>(); PriorityItems = new ConcurrentQueue<T>();
                _pending = new HashSet<T>();
                _isDisposed = false;
            }
        }

        /// <summary>
        /// Starts this renderer.
        /// </summary>
        public void Start()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);

            if (_isRunning) return;
            lock (_syncLock)
            {
                _isRunning = true;
                foreach (var t in _rendererThreads)
                    t.Start(null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void DoRendering(object obj)
        {
            while (_isRunning)
            {
                T item;

                lock (_syncLock)
                {
                    if (PriorityItems.TryDequeue(out item) && _pending.Remove(item) && TryRender(item, out var result))
                    {
                        var args = new RendererEventArgs<T>(item, result, true);
                        MeshCompleted?.Invoke(this, args);
                    }
                    else if (Items.TryDequeue(out item) && _pending.Remove(item) && TryRender(item, out result))
                    {
                        var args = new RendererEventArgs<T>(item, result, false);
                        MeshCompleted?.Invoke(this, args);
                    }
                }

                if (item == null) // We don't have any work, so sleep for a bit.
                    Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected abstract bool TryRender(T item, out Mesh result);

        /// <summary>
        /// Stops this renderer.
        /// </summary>
        public void Stop()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);

            if (!_isRunning) return;
            lock (_syncLock)
            {
                _isRunning = false;
                foreach (var t in _rendererThreads)
                    t.Join();
            }
        }

        /// <summary>
        /// Enqueues an item to this renderer for rendering.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="hasPriority"></param>
        public bool Enqueue(T item, bool hasPriority = false)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);

            if (_pending.Contains(item))
                return false;
            _pending.Add(item);

            if (!_isRunning) return false;
            if (hasPriority)
                PriorityItems.Enqueue(item);
            else
                Items.Enqueue(item);
            return true;
        }

        /// <summary>
        /// Disposes of this renderer.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of this renderer.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            Stop();
            lock (_syncLock)
            {
                _rendererThreads = null;
                Items = null; PriorityItems = null;
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Finalizes this renderer.
        /// </summary>
        ~Renderer()
        {
            Dispose(false);
        }
    }
}
