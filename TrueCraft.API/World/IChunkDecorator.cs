namespace TrueCraft.API.World
{
    /// <summary>
    ///     Used to decorate chunks with "decorations" such as trees, flowers, ores, etc.
    /// </summary>
    public interface IChunkDecorator
    {
        void Decorate(IWorld world, IChunk chunk, IBiomeRepository biomes);
    }
}