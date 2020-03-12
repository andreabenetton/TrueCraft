using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public class RailBlock : BlockProvider, ICraftingRecipe
    {
        public static readonly byte BlockID = 0x42;

        public override byte ID => 0x42;

        public override double BlastResistance => 3.5;

        public override double Hardness => 0.7;

        public override byte Luminance => 0;

        public override bool Opaque => false;

        public override string DisplayName => "Rail";

        public virtual ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {
                        new ItemStack(IronIngotItem.ItemID),
                        ItemStack.EmptyStack,
                        new ItemStack(IronIngotItem.ItemID)
                    },
                    {
                        new ItemStack(IronIngotItem.ItemID),
                        new ItemStack(StickItem.ItemID),
                        new ItemStack(IronIngotItem.ItemID)
                    },
                    {
                        new ItemStack(IronIngotItem.ItemID),
                        ItemStack.EmptyStack,
                        new ItemStack(IronIngotItem.ItemID)
                    }
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(0, 8);
        }
    }

    public class PoweredRailBlock : RailBlock
    {
        public new static readonly byte BlockID = 0x1B;

        public override byte ID => 0x1B;

        public override string DisplayName => "Powered Rail";

        public override ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {
                        new ItemStack(IronIngotItem.ItemID),
                        ItemStack.EmptyStack,
                        new ItemStack(IronIngotItem.ItemID)
                    },
                    {
                        new ItemStack(IronIngotItem.ItemID),
                        new ItemStack(StickItem.ItemID),
                        new ItemStack(IronIngotItem.ItemID)
                    },
                    {
                        new ItemStack(IronIngotItem.ItemID),
                        new ItemStack(RedstoneDustBlock.BlockID),
                        new ItemStack(IronIngotItem.ItemID)
                    }
                };
            }
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(3, 11);
        }
    }

    public class DetectorRailBlock : RailBlock
    {
        public new static readonly byte BlockID = 0x1C;

        public override byte ID => 0x1C;

        public override string DisplayName => "Detector Rail";

        public override ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {
                        new ItemStack(IronIngotItem.ItemID),
                        ItemStack.EmptyStack,
                        new ItemStack(IronIngotItem.ItemID)
                    },
                    {
                        new ItemStack(IronIngotItem.ItemID),
                        new ItemStack(StonePressurePlateBlock.BlockID),
                        new ItemStack(IronIngotItem.ItemID)
                    },
                    {
                        new ItemStack(IronIngotItem.ItemID),
                        new ItemStack(RedstoneDustBlock.BlockID),
                        new ItemStack(IronIngotItem.ItemID)
                    }
                };
            }
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(3, 12);
        }
    }
}