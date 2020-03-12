using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TrueCraft.Core.Collections
{
    public class ByteArraySegment : ICollection<byte>
    {
        private readonly byte[] array;
        private readonly int start;

        public ByteArraySegment(byte[] array, int start, int count)
        {
            this.array = array;
            this.start = start;
            Count = count;
        }

        public byte this[int index]
        {
            get => array[index];
            set
            {
                if (index > array.Length)
                    throw new ArgumentOutOfRangeException("value");

                array[index] = value;
            }
        }

        public void Add(byte item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(byte item)
        {
            return array.Contains(item);
        }

        public void CopyTo(byte[] target, int index)
        {
            Buffer.BlockCopy(array, start, target, index, Count);
        }

        public bool Remove(byte item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }

        public bool IsReadOnly => true;

        public IEnumerator<byte> GetEnumerator()
        {
            return new ByteArraySegmentEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class ByteArraySegmentEnumerator : IEnumerator<byte>
        {
            private readonly ByteArraySegment _segment;
            private int pos;

            public ByteArraySegmentEnumerator(ByteArraySegment segment)
            {
                _segment = segment;
                pos = segment.start;
            }

            public bool MoveNext()
            {
                if (pos >= _segment.Count)
                    return false;

                Current = _segment.array[++pos];

                return true;
            }

            public void Reset()
            {
                pos = _segment.start;
            }

            public byte Current { get; private set; }

            public void Dispose()
            {
            }

            object IEnumerator.Current => Current;
        }
    }
}