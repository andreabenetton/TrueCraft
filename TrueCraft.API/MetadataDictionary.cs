using System.Collections.Generic;
using System.Text;
using TrueCraft.API.Networking;

namespace TrueCraft.API
{
    /// <summary>
    ///     Used to send metadata with entities
    /// </summary>
    public class MetadataDictionary
    {
        private static readonly CreateEntryInstance[] EntryTypes =
        {
            () => new MetadataByte(), // 0
            () => new MetadataShort(), // 1
            () => new MetadataInt(), // 2
            () => new MetadataFloat(), // 3
            () => new MetadataString(), // 4
            () => new MetadataSlot() // 5
        };

        private readonly Dictionary<byte, MetadataEntry> entries;

        public MetadataDictionary()
        {
            entries = new Dictionary<byte, MetadataEntry>();
        }

        public int Count => entries.Count;

        public MetadataEntry this[byte index]
        {
            get => entries[index];
            set => entries[index] = value;
        }

        public static MetadataDictionary FromStream(IMinecraftStream stream)
        {
            var value = new MetadataDictionary();
            while (true)
            {
                var key = stream.ReadUInt8();
                if (key == 127) break;

                var type = (byte) ((key & 0xE0) >> 5);
                var index = (byte) (key & 0x1F);

                var entry = EntryTypes[type]();
                entry.FromStream(stream);
                entry.Index = index;

                value[index] = entry;
            }

            return value;
        }

        public void WriteTo(IMinecraftStream stream)
        {
            foreach (var entry in entries)
                entry.Value.WriteTo(stream, entry.Key);
            stream.WriteUInt8(0x7F);
        }

        public override string ToString()
        {
            StringBuilder sb = null;

            foreach (var entry in entries.Values)
            {
                if (sb != null)
                    sb.Append(", ");
                else
                    sb = new StringBuilder();

                sb.Append(entry);
            }

            if (sb != null)
                return sb.ToString();

            return string.Empty;
        }

        private delegate MetadataEntry CreateEntryInstance();
    }
}