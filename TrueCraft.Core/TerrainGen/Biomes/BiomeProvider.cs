using TrueCraft.API;
using TrueCraft.API.World;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Core.TerrainGen.Biomes
{
    public abstract class BiomeProvider : IBiomeProvider
    {
        /// <summary>
        ///     The ID of the biome.
        /// </summary>
        public abstract byte ID { get; }

        public virtual int Elevation => 0;

        /// <summary>
        ///     The base temperature of the biome.
        /// </summary>
        public abstract double Temperature { get; }

        /// <summary>
        ///     The base rainfall of the biome.
        /// </summary>
        public abstract double Rainfall { get; }

        public virtual bool Spawn => true;

        /// <summary>
        ///     The tree types generated in the biome.
        /// </summary>
        public virtual TreeSpecies[] Trees
        {
            get { return new[] {TreeSpecies.Oak}; }
        }

        /// <summary>
        ///     The flowers generated in the biome.
        /// </summary>
        public virtual PlantSpecies[] Plants
        {
            get { return new[] {PlantSpecies.Dandelion, PlantSpecies.Rose, PlantSpecies.TallGrass, PlantSpecies.Fern}; }
        }

        /// <summary>
        ///     The ores generated in the biome.
        /// </summary>
        public virtual OreTypes[] Ores
        {
            get
            {
                return new[]
                    {OreTypes.Coal, OreTypes.Iron, OreTypes.Lapiz, OreTypes.Redstone, OreTypes.Gold, OreTypes.Diamond};
            }
        }

        /// <summary>
        ///     The required distance between trees.
        /// </summary>
        public virtual double TreeDensity => 4;

        /// <summary>
        ///     The block used to fill water features such as lakes, rivers, etc.
        /// </summary>
        public virtual byte WaterBlock => StationaryWaterBlock.BlockID;

        /// <summary>
        ///     The main surface block used for the terrain of the biome.
        /// </summary>
        public virtual byte SurfaceBlock => GrassBlock.BlockID;

        /// <summary>
        ///     The main "filler" block found under the surface block in the terrain of the biome.
        /// </summary>
        public virtual byte FillerBlock => DirtBlock.BlockID;

        /// <summary>
        ///     The depth of the surface block layer
        /// </summary>
        public virtual int SurfaceDepth => 1;

        /// <summary>
        ///     The depth of the "filler" blocks  located below the surface block layer
        /// </summary>
        public virtual int FillerDepth => 4;
    }
}