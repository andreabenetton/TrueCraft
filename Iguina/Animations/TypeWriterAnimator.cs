using System;
using Iguina.Entities;

namespace Iguina.Animations
{
    /// <summary>
    /// Reveal a <see cref="Paragraph"/>'s text one character at a time, finishing
    /// after <c>fullText.Length / charactersPerSecond</c> seconds. Mirrors
    /// GeonBit.UI's TypeWriterAnimator.
    /// </summary>
    public class TypeWriterAnimator : Animator
    {
        readonly string _fullText;
        readonly float _charactersPerSecond;

        public TypeWriterAnimator(string fullText, float charactersPerSecond = 30f)
        {
            _fullText = fullText ?? string.Empty;
            _charactersPerSecond = MathF.Max(1f, charactersPerSecond);
            Duration = _fullText.Length / _charactersPerSecond;
        }

        protected override void Apply(Entity target, float progress)
        {
            if (target is not Paragraph p) return;
            var visible = Math.Min(_fullText.Length, (int)(ElapsedTime * _charactersPerSecond));
            p.Text = _fullText.Substring(0, visible);
        }
    }
}
