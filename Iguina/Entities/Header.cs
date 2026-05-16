using Iguina.Defs;


namespace Iguina.Entities
{
    /// <summary>
    /// GeonBit.UI compatibility alias for <see cref="Title"/>. Behaviour is
    /// identical; the type exists so code ported verbatim from GeonBit
    /// continues to compile.
    /// </summary>
    public class Header : Title
    {
        public Header(UISystem system, StyleSheet? stylesheet, string text = "New Header", bool ignoreInteractions = true)
            : base(system, stylesheet, text, ignoreInteractions)
        {
        }

        public Header(UISystem system, string text = "New Header", bool ignoreInteractions = true)
            : base(system, text, ignoreInteractions)
        {
        }
    }
}
