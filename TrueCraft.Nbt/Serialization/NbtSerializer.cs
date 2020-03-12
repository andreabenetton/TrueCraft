using System;
using System.Linq;
using TrueCraft.Nbt.Tags;

namespace TrueCraft.Nbt.Serialization
{
    public class NbtSerializer
    {
        /// <summary>
        ///     Decorates the given property or field with the specified
        ///     NBT tag name.
        /// </summary>
        public NbtSerializer(Type type)
        {
            Type = type;
        }

        public Type Type { get; set; }

        public NbtTag Serialize(object value, bool skipInterfaceCheck = false)
        {
            return Serialize(value, "", skipInterfaceCheck);
        }

        public NbtTag Serialize(object value, string tagName, bool skipInterfaceCheck = false)
        {
            if (!skipInterfaceCheck && value is INbtSerializable)
                return ((INbtSerializable) value).Serialize(tagName);
            if (value is NbtTag)
                return (NbtTag) value;
            if (value is byte)
                return new NbtByte(tagName, (byte) value);
            if (value is sbyte)
                return new NbtByte(tagName, (byte) (sbyte) value);
            if (value is bool)
                return new NbtByte(tagName, (byte) ((bool) value ? 1 : 0));
            if (value is byte[])
                return new NbtByteArray(tagName, (byte[]) value);
            if (value is double)
                return new NbtDouble(tagName, (double) value);
            if (value is float f)
                return new NbtFloat(tagName, f);
            if (value is int)
                return new NbtInt(tagName, (int) value);
            if (value is uint)
                return new NbtInt(tagName, (int) (uint) value);
            if (value is int[])
                return new NbtIntArray(tagName, (int[]) value);
            if (value is long)
                return new NbtLong(tagName, (long) value);
            if (value is ulong)
                return new NbtLong(tagName, (long) (ulong) value);
            if (value is short)
                return new NbtShort(tagName, (short) value);
            if (value is ushort)
                return new NbtShort(tagName, (short) (ushort) value);
            if (value is string)
                return new NbtString(tagName, (string) value);
            if (value.GetType().IsArray)
            {
                var elementType = value.GetType().GetElementType();
                var array = value as Array;
                var listType = NbtTagType.Compound;
                if (elementType == typeof(byte) || elementType == typeof(sbyte))
                    listType = NbtTagType.Byte;
                else if (elementType == typeof(bool))
                    listType = NbtTagType.Byte;
                else if (elementType == typeof(byte[]))
                    listType = NbtTagType.ByteArray;
                else if (elementType == typeof(double))
                    listType = NbtTagType.Double;
                else if (elementType == typeof(float))
                    listType = NbtTagType.Float;
                else if (elementType == typeof(int) || elementType == typeof(uint))
                    listType = NbtTagType.Int;
                else if (elementType == typeof(int[]))
                    listType = NbtTagType.IntArray;
                else if (elementType == typeof(long) || elementType == typeof(ulong))
                    listType = NbtTagType.Long;
                else if (elementType == typeof(short) || elementType == typeof(ushort))
                    listType = NbtTagType.Short;
                else if (elementType == typeof(string))
                    listType = NbtTagType.String;
                var list = new NbtList(tagName, listType);
                var innerSerializer = new NbtSerializer(elementType);
                for (var i = 0; i < array.Length; i++)
                    list.Add(innerSerializer.Serialize(array.GetValue(i)));
                return list;
            }

            if (value is NbtFile)
                return ((NbtFile) value).RootTag;
            var compound = new NbtCompound(tagName);

            if (value == null) return compound;
            var nameAttributes = Attribute.GetCustomAttributes(value.GetType(), typeof(TagNameAttribute));

            if (nameAttributes.Length > 0)
                compound = new NbtCompound(((TagNameAttribute) nameAttributes[0]).Name);

            var properties = Type.GetProperties().Where(p => !Attribute.GetCustomAttributes(p,
                typeof(NbtIgnoreAttribute)).Any());

            foreach (var property in properties)
            {
                if (!property.CanRead)
                    continue;

                NbtTag tag = null;

                var name = property.Name;
                nameAttributes = Attribute.GetCustomAttributes(property, typeof(TagNameAttribute));
                var ignoreOnNullAttribute = Attribute.GetCustomAttribute(property, typeof(IgnoreOnNullAttribute));
                if (nameAttributes.Length != 0)
                    name = ((TagNameAttribute) nameAttributes[0]).Name;

                var innerSerializer = new NbtSerializer(property.PropertyType);
                var propValue = property.GetValue(value, null);

                if (propValue == null)
                {
                    if (ignoreOnNullAttribute != null) continue;
                    if (property.PropertyType.IsValueType)
                        propValue = Activator.CreateInstance(property.PropertyType);
                    else if (property.PropertyType == typeof(string))
                        propValue = "";
                }

                tag = innerSerializer.Serialize(propValue, name);
                compound.Add(tag);
            }

            return compound;
        }

        public object Deserialize(NbtTag value, bool skipInterfaceCheck = false)
        {
            if (!skipInterfaceCheck && typeof(INbtSerializable).IsAssignableFrom(Type))
            {
                var instance = (INbtSerializable) Activator.CreateInstance(Type);
                instance.Deserialize(value);
                return instance;
            }

            if (value is NbtByte)
                return ((NbtByte) value).Value;
            if (value is NbtByteArray)
                return ((NbtByteArray) value).Value;
            if (value is NbtDouble)
                return ((NbtDouble) value).Value;
            if (value is NbtFloat)
                return ((NbtFloat) value).Value;
            if (value is NbtInt)
                return ((NbtInt) value).Value;
            if (value is NbtIntArray)
                return ((NbtIntArray) value).Value;
            if (value is NbtLong)
                return ((NbtLong) value).Value;
            if (value is NbtShort)
                return ((NbtShort) value).Value;
            if (value is NbtString)
                return ((NbtString) value).Value;
            if (value is NbtList)
            {
                var list = (NbtList) value;
                var type = typeof(object);
                if (list.ListType == NbtTagType.Byte)
                {
                    type = typeof(byte);
                }
                else if (list.ListType == NbtTagType.ByteArray)
                {
                    type = typeof(byte[]);
                }
                else if (list.ListType == NbtTagType.Compound)
                {
                    if (Type.IsArray)
                        type = Type.GetElementType();
                    else
                        type = typeof(object);
                }
                else if (list.ListType == NbtTagType.Double)
                {
                    type = typeof(double);
                }
                else if (list.ListType == NbtTagType.Float)
                {
                    type = typeof(float);
                }
                else if (list.ListType == NbtTagType.Int)
                {
                    type = typeof(int);
                }
                else if (list.ListType == NbtTagType.IntArray)
                {
                    type = typeof(int[]);
                }
                else if (list.ListType == NbtTagType.Long)
                {
                    type = typeof(long);
                }
                else if (list.ListType == NbtTagType.Short)
                {
                    type = typeof(short);
                }
                else if (list.ListType == NbtTagType.String)
                {
                    type = typeof(string);
                }
                else
                {
                    throw new NotSupportedException("The NBT list type '" + list.TagType + "' is not supported.");
                }

                var array = Array.CreateInstance(type, list.Count);
                var innerSerializer = new NbtSerializer(type);
                for (var i = 0; i < array.Length; i++)
                    array.SetValue(innerSerializer.Deserialize(list[i]), i);
                return array;
            }

            if (value is NbtCompound)
            {
                var compound = value as NbtCompound;
                var properties = Type.GetProperties().Where(p =>
                    !Attribute.GetCustomAttributes(p, typeof(NbtIgnoreAttribute)).Any());
                var resultObject = Activator.CreateInstance(Type);
                foreach (var property in properties)
                {
                    if (!property.CanWrite)
                        continue;
                    var name = property.Name;
                    var nameAttributes = Attribute.GetCustomAttributes(property, typeof(TagNameAttribute));

                    if (nameAttributes.Length != 0)
                        name = ((TagNameAttribute) nameAttributes[0]).Name;
                    var node = compound.Tags.SingleOrDefault(a => a.Name == name);
                    if (node == null) continue;
                    object data;
                    if (typeof(INbtSerializable).IsAssignableFrom(property.PropertyType))
                    {
                        data = Activator.CreateInstance(property.PropertyType);
                        ((INbtSerializable) data).Deserialize(node);
                    }
                    else
                    {
                        data = new NbtSerializer(property.PropertyType).Deserialize(node);
                    }

                    // Some manual casting for edge cases
                    if (property.PropertyType == typeof(bool)
                        && data is byte)
                        data = (byte) data == 1;
                    if (property.PropertyType == typeof(sbyte) && data is byte)
                        data = (sbyte) (byte) data;

                    property.SetValue(resultObject, data, null);
                }

                return resultObject;
            }

            throw new NotSupportedException("The node type '" + value.GetType() + "' is not supported.");
        }
    }
}