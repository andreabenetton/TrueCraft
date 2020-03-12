using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Core.Logic.Items
{
    public class DyeItem : ItemProvider
    {
        public enum DyeType
        {
            InkSac = 0,
            RoseRed = 1,
            CactusGreen = 2,
            CocoaBeans = 3,
            LapisLazuli = 4,
            PurpleDye = 5,
            CyanDye = 6,
            LightGrayDye = 7,
            GrayDye = 8,
            PinkDye = 9,
            LimeDye = 10,
            DandelionYellow = 11,
            LightBlueDye = 12,
            MagentaDye = 13,
            BoneMeal = 14
        }

        public static readonly short ItemID = 0x15F;

        public override short ID => 0x15F;

        public override string DisplayName => "Dye";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            // TODO: Support additional textures
            return new Tuple<int, int>(14, 4);
        }

        public class BoneMealRecipe : ICraftingRecipe
        {
            public ItemStack[,] Pattern
            {
                get { return new[,] {{new ItemStack(BoneItem.ItemID)}}; }
            }

            public ItemStack Output => new ItemStack(ItemID, 1, (short) DyeType.BoneMeal);

            public bool SignificantMetadata => false;
        }

        public class LightGrayDyeRecipe : ICraftingRecipe
        {
            public ItemStack[,] Pattern
            {
                get
                {
                    return new[,]
                    {
                        {
                            new ItemStack(ItemID, 1, (short) DyeType.InkSac),
                            new ItemStack(ItemID, 1, (short) DyeType.BoneMeal),
                            new ItemStack(ItemID, 1, (short) DyeType.BoneMeal)
                        }
                    };
                }
            }

            public ItemStack Output => new ItemStack(ItemID, 1, (short) DyeType.LightGrayDye);

            public bool SignificantMetadata => true;
        }

        public class GrayDyeRecipe : ICraftingRecipe
        {
            public ItemStack[,] Pattern
            {
                get
                {
                    return new[,]
                    {
                        {
                            new ItemStack(ItemID, 1, (short) DyeType.InkSac),
                            ItemStack.EmptyStack,
                            new ItemStack(ItemID, 1, (short) DyeType.BoneMeal)
                        }
                    };
                }
            }

            public ItemStack Output => new ItemStack(ItemID, 1, (short) DyeType.GrayDye);

            public bool SignificantMetadata => true;
        }

        public class RoseRedRecipe : ICraftingRecipe
        {
            public ItemStack[,] Pattern
            {
                get
                {
                    return new[,]
                    {
                        {
                            new ItemStack(RoseBlock.BlockID)
                        }
                    };
                }
            }

            public ItemStack Output => new ItemStack(ItemID, 1, (short) DyeType.RoseRed);

            public bool SignificantMetadata => false;
        }

        public class OrangeDyeRecipe : ICraftingRecipe
        {
            public ItemStack[,] Pattern
            {
                get
                {
                    return new[,]
                    {
                        {
                            new ItemStack(ItemID, 1, (short) DyeType.DandelionYellow),
                            new ItemStack(ItemID, 1, (short) DyeType.RoseRed)
                        }
                    };
                }
            }

            public ItemStack Output => new ItemStack(ItemID, 1, (short) DyeType.RoseRed);

            public bool SignificantMetadata => true;
        }

        public class DandelionYellowRecipe : ICraftingRecipe
        {
            public ItemStack[,] Pattern
            {
                get
                {
                    return new[,]
                    {
                        {
                            new ItemStack(DandelionBlock.BlockID)
                        }
                    };
                }
            }

            public ItemStack Output => new ItemStack(ItemID, 1, (short) DyeType.DandelionYellow);

            public bool SignificantMetadata => false;
        }

        public class LimeDyeRecipe : ICraftingRecipe
        {
            public ItemStack[,] Pattern
            {
                get
                {
                    return new[,]
                    {
                        {
                            new ItemStack(ItemID, 1, (short) DyeType.CactusGreen),
                            ItemStack.EmptyStack,
                            new ItemStack(ItemID, 1, (short) DyeType.BoneMeal)
                        }
                    };
                }
            }

            public ItemStack Output => new ItemStack(ItemID, 1, (short) DyeType.LimeDye);

            public bool SignificantMetadata => true;
        }

        public class LightBlueDyeRecipe : ICraftingRecipe
        {
            public ItemStack[,] Pattern
            {
                get
                {
                    return new[,]
                    {
                        {
                            new ItemStack(ItemID, 1, (short) DyeType.LapisLazuli),
                            ItemStack.EmptyStack,
                            new ItemStack(ItemID, 1, (short) DyeType.BoneMeal)
                        }
                    };
                }
            }

            public ItemStack Output => new ItemStack(ItemID, 1, (short) DyeType.LightBlueDye);

            public bool SignificantMetadata => true;
        }

        public class CyanDyeRecipe : ICraftingRecipe
        {
            public ItemStack[,] Pattern
            {
                get
                {
                    return new[,]
                    {
                        {
                            new ItemStack(ItemID, 1, (short) DyeType.LapisLazuli),
                            ItemStack.EmptyStack,
                            new ItemStack(ItemID, 1, (short) DyeType.CactusGreen)
                        }
                    };
                }
            }

            public ItemStack Output => new ItemStack(ItemID, 1, (short) DyeType.CyanDye);

            public bool SignificantMetadata => true;
        }

        public class PurpleDyeRecipe : ICraftingRecipe
        {
            public ItemStack[,] Pattern
            {
                get
                {
                    return new[,]
                    {
                        {
                            new ItemStack(ItemID, 1, (short) DyeType.LapisLazuli),
                            ItemStack.EmptyStack,
                            new ItemStack(ItemID, 1, (short) DyeType.RoseRed)
                        }
                    };
                }
            }

            public ItemStack Output => new ItemStack(ItemID, 1, (short) DyeType.PurpleDye);

            public bool SignificantMetadata => true;
        }

        public class MagentaDyeRecipe1 : ICraftingRecipe
        {
            public ItemStack[,] Pattern
            {
                get
                {
                    return new[,]
                    {
                        {
                            new ItemStack(ItemID, 1, (short) DyeType.PurpleDye),
                            new ItemStack(ItemID, 1, (short) DyeType.PinkDye)
                        }
                    };
                }
            }

            public ItemStack Output => new ItemStack(ItemID, 1, (short) DyeType.MagentaDye);

            public bool SignificantMetadata => true;
        }

        public class MagentaDyeRecipe2 : ICraftingRecipe
        {
            public ItemStack[,] Pattern
            {
                get
                {
                    return new[,]
                    {
                        {
                            new ItemStack(ItemID, 1, (short) DyeType.LapisLazuli),
                            new ItemStack(ItemID, 1, (short) DyeType.BoneMeal),
                            new ItemStack(ItemID, 2, (short) DyeType.RoseRed)
                        }
                    };
                }
            }

            public ItemStack Output => new ItemStack(ItemID, 1, (short) DyeType.MagentaDye);

            public bool SignificantMetadata => true;
        }

        public class MagentaDyeRecipe3 : ICraftingRecipe
        {
            public ItemStack[,] Pattern
            {
                get
                {
                    return new[,]
                    {
                        {
                            new ItemStack(ItemID, 1, (short) DyeType.LapisLazuli),
                            new ItemStack(ItemID, 1, (short) DyeType.PinkDye),
                            new ItemStack(ItemID, 1, (short) DyeType.RoseRed)
                        }
                    };
                }
            }

            public ItemStack Output => new ItemStack(ItemID, 1, (short) DyeType.MagentaDye);

            public bool SignificantMetadata => true;
        }

        public class PinkDyeRecipe : ICraftingRecipe
        {
            public ItemStack[,] Pattern
            {
                get
                {
                    return new[,]
                    {
                        {
                            new ItemStack(ItemID, 1, (short) DyeType.BoneMeal),
                            new ItemStack(ItemID, 1, (short) DyeType.RoseRed)
                        }
                    };
                }
            }

            public ItemStack Output => new ItemStack(ItemID, 1, (short) DyeType.PinkDye);

            public bool SignificantMetadata => true;
        }
    }
}