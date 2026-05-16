using System;
using Iguina.Entities;

namespace Iguina.Animations
{
    /// <summary>
    /// Continuously oscillate the entity's Y offset between -Amplitude and
    /// +Amplitude pixels with a sine wave of the given Period. Loops forever
    /// (Duration = 0). Mirrors GeonBit.UI's FloatUpDownAnimator.
    /// </summary>
    public class FloatUpDownAnimator : Animator
    {
        public float Amplitude { get; set; } = 4f;
        public float Period { get; set; } = 1.5f;

        readonly float _baseOffsetY;

        public FloatUpDownAnimator(Entity target)
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
