using System;

namespace TrueCraft.Nbt.Snbt
{
    /// <summary>
    ///     Raised when <see cref="SnbtParser"/> fails to parse a stringified-NBT input.
    ///     The <see cref="Position"/> property points to the offset in the source string
    ///     where the parse failed; the message describes what was expected vs. seen.
    /// </summary>
    public sealed class SnbtParseException : Exception
    {
        public SnbtParseException(string message, int position)
            : base($"{message} (at position {position})")
        {
            Position = position;
        }

        /// <summary> Zero-based offset into the SNBT source string where parsing failed. </summary>
        public int Position { get; }
    }
}
