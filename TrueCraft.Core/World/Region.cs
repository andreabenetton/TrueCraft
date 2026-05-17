using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TrueCraft.API;
using TrueCraft.API.World;
using TrueCraft.Core.Networking;
using TrueCraft.Nbt;

namespace TrueCraft.Core.World;

/// <summary>
///     Represents a 32x32 area of <see cref="Chunk" /> objects.
///     Not all of these chunks are represented at any given time, and
///     will be loaded from disk or generated when the need arises.
/// </summary>
public class Region : IDisposable, IRegion
{
    private readonly ILogger<Region> _log;

    // In chunks
    public const int Width = 32, Depth = 32;

    // Single mutual-exclusion primitive shared by the sync and async I/O paths.
    // SemaphoreSlim supports Wait() and WaitAsync() so sync Save/GetChunk and async SaveAsync honor the
    // same exclusion. Note: SemaphoreSlim is *not* reentrant — callers must never re-acquire it from within
    // an already-held critical section. The existing call patterns don't (Save's GetChunk hits the dict on
    // dirty chunks and never reaches GetChunk's own lock acquisition).
    private readonly SemaphoreSlim streamLock = new SemaphoreSlim(1, 1);

    /// <summary>
    ///     Creates a new Region for server-side use at the given position using
    ///     the provided terrain generator.
    /// </summary>
    public Region(Coordinates2D position, World world, ILogger<Region> log = null)
    {
        _log = log ?? NullLogger<Region>.Instance;
        _Chunks = new ConcurrentDictionary<Coordinates2D, IChunk>();
        Position = position;
        World = world;
    }

    /// <summary>
    ///     Creates a region from the given region file.
    /// </summary>
    public Region(Coordinates2D position, World world, string file, ILogger<Region> log = null)
        : this(position, world, log)
    {
        _log.LogDebug("Region({Pos}) ctor file={File}, fileExists={Exists}", position, file, File.Exists(file));
        if (File.Exists(file))
        {
            _log.LogDebug("Region({Pos}) opening existing file", position);
            regionFile = File.Open(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            _log.LogDebug("Region({Pos}) file opened, reading 8KB header", position);
            regionFile.ReadExactly(HeaderCache, 0, 8192);
            _log.LogDebug("Region({Pos}) header read", position);
        }
        else
        {
            _log.LogDebug("Region({Pos}) creating new file", position);
            regionFile = File.Open(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            CreateRegionHeader();
        }
    }


    /// <summary>
    ///     Asynchronously creates a region from the given region file, opening it with overlapped I/O and
    ///     reading the 8 KiB header table.
    /// </summary>
    public static async Task<Region> CreateAsync(Coordinates2D position, World world, string file,
        CancellationToken cancellationToken = default)
    {
        var region = new Region(position, world);
        var exists = File.Exists(file);
        region.regionFile = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite,
            4096, FileOptions.Asynchronous);
        if (exists)
        {
            await region.regionFile.ReadExactlyAsync(region.HeaderCache.AsMemory(0, 8192), cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            await region.CreateRegionHeaderAsync(cancellationToken).ConfigureAwait(false);
        }
        return region;
    }

    private ConcurrentDictionary<Coordinates2D, IChunk> _Chunks { get; }

    public World World { get; set; }

    private HashSet<Coordinates2D> DirtyChunks { get; } = new HashSet<Coordinates2D>();
    private Stream regionFile { get; set; }

    public void Dispose()
    {
        if (regionFile is null)
            return;
        streamLock.Wait();
        try
        {
            regionFile.Flush();
            regionFile.Close();
        }
        finally
        {
            streamLock.Release();
        }
    }

    /// <summary>
    ///     The currently loaded chunk list.
    /// </summary>
    public IDictionary<Coordinates2D, IChunk> Chunks => _Chunks;

    /// <summary>
    ///     The location of this region in the overworld.
    /// </summary>
    public Coordinates2D Position { get; set; }

    public void DamageChunk(Coordinates2D coords)
    {
        var x = coords.X / Width - (coords.X < 0 ? 1 : 0);
        var z = coords.Z / Depth - (coords.Z < 0 ? 1 : 0);
        DirtyChunks.Add(new Coordinates2D(coords.X - x * 32, coords.Z - z * 32));
    }

    /// <summary>
    ///     Retrieves the requested chunk from the region, or
    ///     generates it if a world generator is provided.
    /// </summary>
    /// <param name="position">The position of the requested local chunk coordinates.</param>
    public IChunk GetChunk(Coordinates2D position, bool generate = true)
    {
        _log.LogDebug("Region({Region}).GetChunk({Local}) start, cached={Cached}", Position, position, Chunks.ContainsKey(position));
        if (!Chunks.ContainsKey(position))
        {
            if (regionFile is not null)
            {
                _log.LogDebug("Region({Region}).GetChunk({Local}) looking up table", Position, position);
                var chunkData = GetChunkFromTable(position);
                _log.LogDebug("Region({Region}).GetChunk({Local}) table result null={Null}", Position, position, chunkData is null);
                if (chunkData is null)
                {
                    if (World.ChunkProvider is null)
                        throw new ArgumentException("The requested chunk is not loaded.", "position");
                    if (generate)
                    {
                        _log.LogDebug("Region({Region}).GetChunk({Local}) generating", Position, position);
                        GenerateChunk(position);
                    }
                    else
                        return null;
                    return Chunks[position];
                }

                _log.LogDebug("Region({Region}).GetChunk({Local}) acquiring streamLock", Position, position);
                streamLock.Wait();
                _log.LogDebug("Region({Region}).GetChunk({Local}) streamLock acquired", Position, position);
                try
                {
                    _log.LogDebug("Region({Region}).GetChunk({Local}) seeking to offset {Offset}", Position, position, chunkData.Item1);
                    regionFile.Seek(chunkData.Item1, SeekOrigin.Begin);
                    /*int length = */
                    new MinecraftStream(regionFile)
                        .ReadInt32(); // TODO: Avoid making new objects here, and in the WriteInt32
                    var compressionMode = regionFile.ReadByte();
                    _log.LogDebug("Region({Region}).GetChunk({Local}) compressionMode={Mode}", Position, position, compressionMode);
                    switch (compressionMode)
                    {
                        case 1: // gzip
                            throw new NotImplementedException("gzipped chunks are not implemented");
                        case 2: // zlib
                            _log.LogDebug("Region({Region}).GetChunk({Local}) loading NBT zlib stream", Position, position);
                            var nbt = new NbtFile();
                            nbt.LoadFromStream(regionFile, NbtCompression.ZLib, null);
                            _log.LogDebug("Region({Region}).GetChunk({Local}) NBT loaded, parsing Chunk", Position, position);
                            var chunk = Chunk.FromNbt(nbt);
                            chunk.ParentRegion = this;
                            Chunks[position] = chunk;
                            _log.LogDebug("Region({Region}).GetChunk({Local}) chunk parsed", Position, position);
                            World.OnChunkLoaded(new ChunkLoadedEventArgs(chunk));
                            break;
                        default:
                            throw new InvalidDataException("Invalid compression scheme provided by region file.");
                    }
                }
                finally
                {
                    streamLock.Release();
                    _log.LogDebug("Region({Region}).GetChunk({Local}) streamLock released", Position, position);
                }
            }
            else if (World.ChunkProvider is null)
            {
                throw new ArgumentException("The requested chunk is not loaded.", nameof(position));
            }
            else
            {
                if (generate)
                    GenerateChunk(position);
                else
                    return null;
            }
        }

        _log.LogDebug("Region({Region}).GetChunk({Local}) done", Position, position);
        return Chunks[position];
    }

    /// <summary>
    ///     Saves this region to the specified file.
    /// </summary>
    public void Save(string file)
    {
        if (File.Exists(file))
        {
            regionFile = regionFile ?? File.Open(file, FileMode.OpenOrCreate);
        }
        else
        {
            regionFile = regionFile ?? File.Open(file, FileMode.OpenOrCreate);
            CreateRegionHeader();
        }

        Save();
    }

    public void UnloadChunk(Coordinates2D position)
    {
        Chunks.Remove(position);
    }

    public void GenerateChunk(Coordinates2D position)
    {
        var globalPosition = Position * new Coordinates2D(Width, Depth) + position;
        var chunk = World.ChunkProvider.GenerateChunk(World, globalPosition);
        chunk.IsModified = true;
        chunk.Coordinates = globalPosition;
        chunk.ParentRegion = this;
        DirtyChunks.Add(position);
        Chunks[position] = chunk;
        World.OnChunkGenerated(new ChunkLoadedEventArgs(chunk));
    }

    /// <summary>
    ///     Sets the chunk at the specified local position to the given value.
    /// </summary>
    public void SetChunk(Coordinates2D position, IChunk chunk)
    {
        Chunks[position] = chunk;
        chunk.IsModified = true;
        DirtyChunks.Add(position);
        chunk.ParentRegion = this;
    }

    /// <summary>
    ///     Saves this region to the open region file.
    /// </summary>
    public void Save()
    {
        streamLock.Wait();
        try
        {
            SaveCore();
        }
        finally
        {
            streamLock.Release();
        }
    }


    /// <summary>
    ///     Asynchronously saves this region to the open region file. Tag serialization and the region-table
    ///     allocator stay synchronous (CPU-bound, no I/O on the hot path); the actual file writes are async.
    /// </summary>
    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await streamLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await SaveCoreAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            streamLock.Release();
        }
    }


    /// <summary>
    ///     Asynchronously saves this region to the specified file, opening it for async I/O if not already open.
    /// </summary>
    public async Task SaveAsync(string file, CancellationToken cancellationToken = default)
    {
        if (regionFile is null)
        {
            var exists = File.Exists(file);
            regionFile = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite,
                4096, FileOptions.Asynchronous);
            if (!exists)
                await CreateRegionHeaderAsync(cancellationToken).ConfigureAwait(false);
        }
        await SaveAsync(cancellationToken).ConfigureAwait(false);
    }


    // Assumes streamLock is already held by caller.
    private void SaveCore()
    {
        var toRemove = new List<Coordinates2D>();
        var chunks = DirtyChunks.ToList();
        DirtyChunks.Clear();
        foreach (var coords in chunks)
        {
            // Dirty chunks are always already loaded, so this is a dict lookup with no nested lock.
            var chunk = GetChunk(coords, false);
            if (chunk.IsModified)
            {
                var data = ((Chunk) chunk).ToNbt();
                var raw = data.SaveToBuffer(NbtCompression.ZLib);

                var header = GetChunkFromTable(coords);
                if (header is null || header.Item2 > raw.Length)
                    header = AllocateNewChunks(coords, raw.Length);

                regionFile.Seek(header.Item1, SeekOrigin.Begin);
                new MinecraftStream(regionFile).WriteInt32(raw.Length);
                regionFile.WriteByte(2); // Compressed with zlib
                regionFile.Write(raw, 0, raw.Length);

                chunk.IsModified = false;
            }

            if ((DateTime.UtcNow - chunk.LastAccessed).TotalMinutes > 5)
                toRemove.Add(coords);
        }

        regionFile.Flush();
        // Unload idle chunks
        foreach (var c in toRemove)
        {
            var inst = Chunks[c];
            Chunks.Remove(c);
            inst.Dispose();
        }
    }


    // Assumes streamLock is already held by caller.
    private async Task SaveCoreAsync(CancellationToken cancellationToken)
    {
        var toRemove = new List<Coordinates2D>();
        var chunks = DirtyChunks.ToList();
        DirtyChunks.Clear();
        // Reused 4-byte buffer for big-endian length prefix and 1-byte zlib marker.
        var lengthBuf = new byte[4];
        var markerBuf = new byte[1] { 2 };
        foreach (var coords in chunks)
        {
            var chunk = GetChunk(coords, false);
            if (chunk.IsModified)
            {
                var data = ((Chunk) chunk).ToNbt();
                var raw = data.SaveToBuffer(NbtCompression.ZLib);

                var header = GetChunkFromTable(coords);
                if (header is null || header.Item2 > raw.Length)
                    header = AllocateNewChunks(coords, raw.Length);

                regionFile.Seek(header.Item1, SeekOrigin.Begin);
                lengthBuf[0] = (byte) (raw.Length >> 24);
                lengthBuf[1] = (byte) (raw.Length >> 16);
                lengthBuf[2] = (byte) (raw.Length >> 8);
                lengthBuf[3] = (byte) raw.Length;
                await regionFile.WriteAsync(lengthBuf.AsMemory(0, 4), cancellationToken).ConfigureAwait(false);
                await regionFile.WriteAsync(markerBuf.AsMemory(0, 1), cancellationToken).ConfigureAwait(false);
                await regionFile.WriteAsync(raw.AsMemory(0, raw.Length), cancellationToken).ConfigureAwait(false);

                chunk.IsModified = false;
            }

            if ((DateTime.UtcNow - chunk.LastAccessed).TotalMinutes > 5)
                toRemove.Add(coords);
        }

        await regionFile.FlushAsync(cancellationToken).ConfigureAwait(false);
        foreach (var c in toRemove)
        {
            var inst = Chunks[c];
            Chunks.Remove(c);
            inst.Dispose();
        }
    }

    public static string GetRegionFileName(Coordinates2D position)
    {
        return $"r.{position.X}.{position.Z}.mca";
    }

    #region Stream Helpers

    private const int ChunkSizeMultiplier = 4096;
    private byte[] HeaderCache = new byte[8192];

    private Tuple<int, int> GetChunkFromTable(Coordinates2D position) // <offset, length>
    {
        var tableOffset = (position.X % Width + position.Z % Depth * Width) * 4;
        var offsetBuffer = new byte[4];
        Buffer.BlockCopy(HeaderCache, tableOffset, offsetBuffer, 0, 3);
        Array.Reverse(offsetBuffer);
        int length = HeaderCache[tableOffset + 3];
        var offset = BitConverter.ToInt32(offsetBuffer, 0) << 4;
        if (offset == 0 || length == 0)
            return null;
        return new Tuple<int, int>(offset,
            length * ChunkSizeMultiplier);
    }

    private void CreateRegionHeader()
    {
        HeaderCache = new byte[8192];
        regionFile.Write(HeaderCache, 0, 8192);
        regionFile.Flush();
    }


    private async Task CreateRegionHeaderAsync(CancellationToken cancellationToken)
    {
        HeaderCache = new byte[8192];
        await regionFile.WriteAsync(HeaderCache.AsMemory(0, 8192), cancellationToken).ConfigureAwait(false);
        await regionFile.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private Tuple<int, int> AllocateNewChunks(Coordinates2D position, int length)
    {
        // Expand region file
        regionFile.Seek(0, SeekOrigin.End);
        var dataOffset = (int) regionFile.Position;

        length /= ChunkSizeMultiplier;
        length++;
        regionFile.Write(new byte[length * ChunkSizeMultiplier], 0, length * ChunkSizeMultiplier);

        // Write table entry
        var tableOffset = (position.X % Width + position.Z % Depth * Width) * 4;
        regionFile.Seek(tableOffset, SeekOrigin.Begin);

        var entry = BitConverter.GetBytes(dataOffset >> 4);
        entry[0] = (byte) length;
        Array.Reverse(entry);
        regionFile.Write(entry, 0, entry.Length);
        Buffer.BlockCopy(entry, 0, HeaderCache, tableOffset, 4);

        return new Tuple<int, int>(dataOffset, length * ChunkSizeMultiplier);
    }

    #endregion
}
