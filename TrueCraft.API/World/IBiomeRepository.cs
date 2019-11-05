namespace TrueCraft.API.World
{
    public interface IBiomeRepository
    {
        IBiomeProvider GetBiome(byte id);
        IBiomeProvider GetBiome(double temperature, double rainfall, bool spawn);
        void RegisterBiomeProvider(IBiomeProvider provider);
    }
}