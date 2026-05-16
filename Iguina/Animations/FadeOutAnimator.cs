using Iguina.Defs;
using Iguina.Entities;

namespace Iguina.Animations
{
    /// <summary>
    /// Fade an entity's tint alpha from opaque to transparent over <c>Duration</c>
    /// seconds, then optionally hide it. Mirrors GeonBit.UI's FadeOutAnimator.
    /// </summary>
    public class FadeOutAnimator : Animator
    {
        /// <summary>If true, the entity's <see cref="Entity.Visible"/> is set to
        /// false once the fade completes.</summary>
        public bool HideOnComplete { get; set; } = true;

        public FadeOutAnimator(float duration)
        {
            Duration = duration;
        }

        protected override void Apply(Entity target, float progress)
        {
            var alpha = (byte)(255 * (1f - progress));
            target.OverrideStyles.TintColor = new Color(255, 255, 255, alpha);
            if (IsDone && HideOnComplete) target.Visible = false;
        }
    }
}
