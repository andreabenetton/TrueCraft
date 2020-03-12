using System;
using TrueCraft.API;

namespace TrueCraft.Core.Entities
{
    public class SlimeEntity : MobEntity
    {
        public SlimeEntity() : this(4)
        {
        }

        public SlimeEntity(byte size)
        {
            SlimeSize = size;
        }

        public byte SlimeSize { get; set; }

        public override MetadataDictionary Metadata
        {
            get
            {
                var meta = base.Metadata;
                meta[16] = new MetadataByte(SlimeSize);
                return meta;
            }
        }

        public override Size Size => new Size(0.6 * SlimeSize);

        public override short MaxHealth => (short) Math.Pow(SlimeSize, 2);

        public override sbyte MobType => 55;

        public override bool Friendly => false;
    }
}