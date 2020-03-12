using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.API.Windows;

namespace TrueCraft.Core.Windows
{
    public class InventoryWindow : Window
    {
        public InventoryWindow(ICraftingRepository craftingRepository)
        {
            WindowAreas = new[]
            {
                new CraftingWindowArea(craftingRepository, CraftingOutputIndex),
                new ArmorWindowArea(ArmorIndex),
                new WindowArea(MainIndex, 27, 9, 3), // Main inventory
                new WindowArea(HotbarIndex, 9, 9, 1) // Hotbar
            };
            foreach (var area in WindowAreas)
                area.WindowChange += (s, e) => OnWindowChange(new WindowChangeEventArgs(
                    (s as WindowArea).StartIndex + e.SlotIndex, e.Value));
        }

        public override void CopyToInventory(IWindow inventoryWindow)
        {
            // This space intentionally left blank
        }

        protected override IWindowArea GetLinkedArea(int index, ItemStack slot)
        {
            if (index == 0 || index == 1 || index == 3)
                return MainInventory;
            return Hotbar;
        }

        public override bool PickUpStack(ItemStack slot)
        {
            var area = MainInventory;
            foreach (var item in Hotbar.Items)
                if (item.Empty || slot.ID == item.ID && slot.Metadata == item.Metadata)
                    //&& item.Count + slot.Count < Item.GetMaximumStackSize(new ItemDescriptor(item.Id, item.Metadata)))) // TODO
                {
                    area = Hotbar;
                    break;
                }

            var index = area.MoveOrMergeItem(-1, slot, null);
            return index != -1;
        }

        #region Variables

        public const short HotbarIndex = 36;
        public const short CraftingGridIndex = 1;
        public const short CraftingOutputIndex = 0;
        public const short ArmorIndex = 5;
        public const short MainIndex = 9;

        public override string Name => "Inventory";

        public override sbyte Type => -1; // NOTE: This window does not have a type

        public override short[] ReadOnlySlots
        {
            get { return new[] {CraftingOutputIndex}; }
        }

        public override IWindowArea[] WindowAreas { get; protected set; }

        #region Properties

        public IWindowArea CraftingGrid => WindowAreas[0];

        public IWindowArea Armor => WindowAreas[1];

        public IWindowArea MainInventory => WindowAreas[2];

        public IWindowArea Hotbar => WindowAreas[3];

        #endregion

        #endregion
    }
}