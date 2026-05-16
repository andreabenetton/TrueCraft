using System;
using Iguina.Entities;

namespace Iguina.Animations;

/// <summary>
/// Bob each glyph of a <see cref="Paragraph"/> vertically in a sine wave;
/// adjacent characters are offset in phase so the wave appears to travel
/// across the text. Mirrors GeonBit.UI's TextWaveAnimator behaviour.
///
/// Drives <see cref="Paragraph.PerGlyphOffsetY"/>; clearing it again on
/// removal restores normal whole-line rendering. Loops forever
/// (<see cref="Animator.Duration"/> == 0).
/// </summary>
public class TextWaveAnimator : Animator
{
    /// <summary>Peak vertical offset, in pixels.</summary>
    public float Amplitude { get; set; } = 4f;

    /// <summary>Period of one full sine cycle, in seconds.</summary>
    public float Period { get; set; } = 0.6f;

    /// <summary>Phase delta between adjacent characters, in radians. Higher
    /// values make the wave appear to travel faster across the text.</summary>
    public float CharacterPhase { get; set; } = 0.6f;

    readonly Paragraph _target;
    bool _installed;

    public TextWaveAnimator(Paragraph target)
    {
        Duration = 0f;
        _target = target;
    }

    protected override void Apply(Entity target, float elapsedSeconds)
    {
        if (!_installed)
        {
            _target.PerGlyphOffsetY = GetOffset;
            _installed = true;
        }
        // Apply is called for its side effect; the actual per-glyph values are
        // pulled from the callback by Paragraph at draw time, so we just need
        // to keep ElapsedTime current — the base class already did that.
    }

    float GetOffset(int charIndex)
    {
        var t = ElapsedTime / Period * MathF.PI * 2f + charIndex * CharacterPhase;
        return MathF.Sin(t) * Amplitude;
    }

    /// <summary>Call to remove the animator's hook and stop the effect.
    /// Sets <see cref="Animator.IsDone"/> so the entity loop drops it on the
    /// next frame.</summary>
    public void Stop()
    {
        if (_target.PerGlyphOffsetY == GetOffset)
            _target.PerGlyphOffsetY = null;
        IsDone = true;
    }
}
