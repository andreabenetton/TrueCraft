using System;

namespace TrueCraft.Client.Events
{
    public class ChunkEventArgs : EventArgs
    {
        public ChunkEventArgs(ReadOnlyChunk chunk)
        {
            Chunk = chunk;
        }

        public ReadOnlyChunk Chunk { get; set; }
    }
}