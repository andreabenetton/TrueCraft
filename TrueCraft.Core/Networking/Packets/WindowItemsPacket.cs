using TrueCraft.API;
using TrueCraft.API.Networking;

namespace TrueCraft.Core.Networking.Packets
{
    /// <summary>
    ///     Sets the items in an inventory window on the client.
    /// </summary>
    public struct WindowItemsPacket : IPacket
    {
        public byte ID => 0x68;

        public WindowItemsPacket(sbyte windowID, ItemStack[] items)
        {
            WindowID = windowID;
            Items = items;
        }

        public sbyte WindowID;
        public ItemStack[] Items;

        public void ReadPacket(IMinecraftStream stream)
        {
            WindowID = stream.ReadInt8();
            var length = stream.ReadInt16();
            Items = new ItemStack[length];
            for (var i = 0; i < length; i++)
            {
                var id = stream.ReadInt16();
                if (id != -1)
                {
                    var count = stream.ReadInt8();
                    var metadata = stream.ReadInt16();
                    Items[i] = new ItemStack(id, count, metadata);
                }
                else
                {
                    Items[i] = ItemStack.EmptyStack;
                }
            }
        }

        public void WritePacket(IMinecraftStream stream)
        {
            stream.WriteInt8(WindowID);
            stream.WriteInt16((short) Items.Length);
            for (var i = 0; i < Items.Length; i++)
            {
                stream.WriteInt16(Items[i].ID);
                if (!Items[i].Empty)
                {
                    stream.WriteInt8(Items[i].Count);
                    stream.WriteInt16(Items[i].Metadata);
                }
            }
        }
    }
}