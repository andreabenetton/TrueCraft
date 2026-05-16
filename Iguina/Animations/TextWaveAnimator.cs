using System;
using Iguina.Entities;

namespace Iguina.Animations
{
    /// <summary>
    /// Bob a Paragraph vertically in a sine wave to draw the eye (e.g. for a
    /// notification or callout). Simpler than GeonBit's per-character wave — that
    /// would require per-glyph rendering Iguina doesn't expose — but achieves a
    /// similar attention-grabbing effect on the whole text block.
    /// </summary>
    public class TextWaveAnimator : Animator
    {
        public float Amplitude { get; set; } = 3f;
        public float Period { get; set; } = 0.6f;

        readonly float _baseOffsetY;

        public TextWaveAnimator(Entity target)
        {
            Duration = 0f;
            _baseOffsetY = target.Offset.Y.Value;
        }

        protected override void Apply(Entity target, float elapsedSeconds)
        {
            var phase = (elapsedSeconds / Period) * MathF.PI * 2f;
            var dy = MathF.Sin(phase) * Amplitude;
            target.Offset.Y.SetPixels((int)(_baseOffsetY + dy));
        }
    }
}
