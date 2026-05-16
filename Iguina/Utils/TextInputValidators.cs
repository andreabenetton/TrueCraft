using System;
using System.Globalization;
using System.Linq;

namespace Iguina.Utils
{
    /// <summary>
    ///     Per-keystroke text validator. <paramref name="text"/> is the candidate
    ///     post-insert string and may be transformed by the validator. Return false
    ///     to reject the insertion atomically.
    /// </summary>
    public delegate bool TextValidator(ref string text, string oldText);

    /// <summary>
    ///     Ports of the six GeonBit.UI 4.3 text validators
    ///     (<c>GeonBit.UI.Entities.TextValidators</c>) used by the launcher views.
    ///     Each factory returns a <see cref="TextValidator"/> suitable for
    ///     <see cref="Iguina.Entities.TextInput.Validators"/>.
    /// </summary>
    public static class TextInputValidators
    {
        /// <summary>
        ///     Accept only ASCII letters and digits; optionally allow the space character.
        /// </summary>
        public static TextValidator EnglishCharactersOnly(bool allowSpaces = false)
            => (ref string text, string _) =>
                text.All(c => (c >= 'A' && c <= 'Z')
                           || (c >= 'a' && c <= 'z')
                           || (c >= '0' && c <= '9')
                           || (allowSpaces && c == ' '));

        /// <summary>
        ///     Accept only characters legal in filenames on Windows / Linux / macOS:
        ///     letters, digits, and the punctuation set <c>.-_()[]{}</c>. Optionally
        ///     allow spaces. Rejects the path separators <c>/ \</c> and the reserved
        ///     characters <c>&lt; &gt; : " | ? *</c> plus control codes.
        /// </summary>
        public static TextValidator FilenameValidator(bool allowSpaces = false)
            => (ref string text, string _) =>
                text.All(c =>
                {
                    if (char.IsLetterOrDigit(c)) return true;
                    if (allowSpaces && c == ' ') return true;
                    return c is '.' or '-' or '_' or '(' or ')' or '[' or ']' or '{' or '}';
                });

        /// <summary>
        ///     Reject any value that contains two or more consecutive spaces.
        /// </summary>
        public static TextValidator OnlySingleSpaces()
            => (ref string text, string _) => !text.Contains("  ");

        /// <summary>
        ///     Transform the value to title-case (first letter of each word
        ///     uppercased) on every keystroke. Never rejects.
        /// </summary>
        public static TextValidator MakeTitleCase()
            => (ref string text, string _) =>
            {
                if (string.IsNullOrEmpty(text)) return true;
                text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
                return true;
            };

        /// <summary>
        ///     Accept only numeric input. When <paramref name="allowDecimal"/> is true,
        ///     a single decimal separator (the culture's separator) and a leading
        ///     <c>-</c> are also allowed. When <paramref name="min"/> or
        ///     <paramref name="max"/> are provided and the value parses cleanly, also
        ///     reject out-of-range numbers (empty values are always accepted so the
        ///     user can backspace to nothing).
        /// </summary>
        public static TextValidator NumbersOnly(bool allowDecimal = false, double? min = null, double? max = null)
        {
            var dec = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            return (ref string text, string _) =>
            {
                if (text.Length == 0) return true;

                var seenDecimal = false;
                for (var i = 0; i < text.Length; i++)
                {
                    var c = text[i];
                    if (c >= '0' && c <= '9') continue;
                    if (i == 0 && c == '-') continue;
                    if (allowDecimal && !seenDecimal && i + dec.Length <= text.Length
                        && text.Substring(i, dec.Length) == dec)
                    {
                        seenDecimal = true;
                        i += dec.Length - 1;
                        continue;
                    }
                    return false;
                }

                if ((min.HasValue || max.HasValue)
                    && double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var value))
                {
                    if (min.HasValue && value < min.Value) return false;
                    if (max.HasValue && value > max.Value) return false;
                }
                return true;
            };
        }

        /// <summary>
        ///     Accept only characters valid in a URL slug: ASCII letters, digits,
        ///     hyphen, and underscore. Optionally allow spaces.
        /// </summary>
        public static TextValidator SlugValidator(bool allowSpaces = false)
            => (ref string text, string _) =>
                text.All(c => (c >= 'A' && c <= 'Z')
                           || (c >= 'a' && c <= 'z')
                           || (c >= '0' && c <= '9')
                           || c == '-' || c == '_'
                           || (allowSpaces && c == ' '));
    }
}
