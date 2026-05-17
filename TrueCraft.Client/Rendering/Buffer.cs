using System;
using System.Buffers;

namespace TrueCraft.Client.Rendering;

/// <summary>
///     A thin growable buffer over an ArrayPool-rented <c>T[]</c>.
///     Like <see cref="System.Collections.Generic.List{T}"/> but exposes
///     the backing array so it can be handed straight to GPU upload paths
///     (<see cref="Microsoft.Xna.Framework.Graphics.VertexBuffer.SetData{T}(T[], int, int)"/>)
///     without an intermediate <c>ToArray()</c> copy.
///
///     <para>
///         <see cref="Detach"/> transfers ownership of the current array
///         to the caller and re-rents a fresh array for further
///         <see cref="Add"/>s. The caller is responsible for returning
///         the array to <see cref="ArrayPool{T}.Shared"/> once it is
///         done with it (typically when the consuming mesh is disposed).
///     </para>
/// </summary>
public sealed class Buffer<T>
{
    private T[] _items;
    private int _count;

    public Buffer(int initialCapacity = 4096)
    {
        if (initialCapacity < 1) initialCapacity = 1;
        _items = ArrayPool<T>.Shared.Rent(initialCapacity);
    }

    public int Count => _count;

    /// <summary>The backing storage. May be larger than <see cref="Count"/>.</summary>
    public T[] Array => _items;

    public void Add(T item)
    {
        if (_count >= _items.Length)
            Grow(_count + 1);
        _items[_count++] = item;
    }

    public void EnsureCapacity(int min)
    {
        if (_items.Length < min)
            Grow(min);
    }

    public void Clear()
    {
        _count = 0;
    }

    /// <summary>
    ///     Hands off the current array to a downstream owner. The buffer
    ///     immediately rents a fresh array of equal capacity so subsequent
    ///     <see cref="Add"/>s don't touch the handed-off array.
    ///
    ///     The caller becomes responsible for returning the detached
    ///     array to <see cref="ArrayPool{T}.Shared"/> when finished.
    /// </summary>
    public (T[] array, int count) Detach()
    {
        var arr = _items;
        var n = _count;
        _items = ArrayPool<T>.Shared.Rent(arr.Length);
        _count = 0;
        return (arr, n);
    }

    private void Grow(int min)
    {
        var newCap = _items.Length * 2;
        if (newCap < min) newCap = min;
        var newArr = ArrayPool<T>.Shared.Rent(newCap);
        System.Array.Copy(_items, newArr, _count);
        ArrayPool<T>.Shared.Return(_items);
        _items = newArr;
    }
}
