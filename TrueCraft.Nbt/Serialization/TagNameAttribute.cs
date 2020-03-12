using System;

namespace TrueCraft.Nbt.Serialization
{
    /// <summary>
    ///     Decorates the given property or field with the specified NBT tag name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
    public class TagNameAttribute : Attribute
    {
        /// <summary>
        ///     Decorates the given property or field with the specified NBT tag name.
        /// </summary>
        public TagNameAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        ///     Get or set the specified NBT tag name.
        /// </summary>
        public string Name { get; set; }
    }
}