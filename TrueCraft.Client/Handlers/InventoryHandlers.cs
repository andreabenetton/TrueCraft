using TrueCraft.API.Networking;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.API.Windows;
using TrueCraft.API;
using TrueCraft.Core.Windows;

namespace TrueCraft.Client.Handlers
{
    internal static class InventoryHandlers
    {
        public static void HandleWindowItems(IPacket packet, MultiplayerClient client)
        {
            var windowItemsPacket = (WindowItemsPacket)packet;
            if (windowItemsPacket.WindowID == 0)
                client.Inventory.SetSlots(windowItemsPacket.Items);
            else
                client.CurrentWindow.SetSlots(windowItemsPacket.Items);
        }

        public static void HandleSetSlot(IPacket packet, MultiplayerClient client)
        {
            var setSlotPacket = (SetSlotPacket)packet;
            IWindow window = null;
            if (setSlotPacket.WindowID == 0)
                window = client.Inventory;
            else
                window = client.CurrentWindow;
            if (window != null)
            {
                if (setSlotPacket.SlotIndex >= 0 && setSlotPacket.SlotIndex < window.Length)
                {
                    window[setSlotPacket.SlotIndex] = new ItemStack(setSlotPacket.ItemID, setSlotPacket.Count, setSlotPacket.Metadata);
                }
            }
        }

        public static void HandleOpenWindowPacket(IPacket packet, MultiplayerClient client)
        {
            var openWindowPacket = (OpenWindowPacket)packet;
            IWindow window = null;
            switch (openWindowPacket.Type)
            {
                case 1: // Crafting bench window
                    window = new CraftingBenchWindow(client.CraftingRepository, client.Inventory);
                    break;
            }
            window.ID = openWindowPacket.WindowID;
            client.CurrentWindow = window;
        }

        public static void HandleCloseWindowPacket(IPacket packet, MultiplayerClient client)
        {
            client.CurrentWindow = null;
        }
    }
}