using System;

namespace TrueCraft.Nbt.Serialization
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class IgnoreOnNullAttribute : Attribute
    {
    }
}