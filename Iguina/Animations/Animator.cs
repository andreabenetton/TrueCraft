using Iguina.Entities;

namespace Iguina.Animations;

/// <summary>
/// Base class for time-driven entity animations. Animators are stored on an
/// entity via <see cref="Entity.Animators"/> and ticked once per
/// <see cref="UISystem.Update"/> frame. When <see cref="IsDone"/> goes true
/// the animator is removed from the list.
/// </summary>
public abstract class Animator
{
    /// <summary>Seconds since this animator was added.</summary>
    public float ElapsedTime { get; protected set; }

    /// <summary>Total duration in seconds. Animators may interpret 0 as "loop forever".</summary>
    public float Duration { get; protected set; }

    /// <summary>True when the animator has completed and should be removed.</summary>
    public bool IsDone { get; protected set; }

    /// <summary>Drive one frame of the animation against the owning entity.</summary>
    public virtual void Tick(Entity target, float deltaTime)
    {
        ElapsedTime += deltaTime;
        if (Duration > 0f && ElapsedTime >= Duration)
        {
            ElapsedTime = Duration;
            IsDone = true;
        }
        Apply(target, Duration > 0f ? ElapsedTime / Duration : ElapsedTime);
    }

    /// <param name="target">Entity the animator is mounted on.</param>
    /// <param name="progress">For finite-duration animators, [0,1]. For looping
    /// animators (<see cref="Duration"/> == 0), the raw elapsed-time in seconds.</param>
    protected abstract void Apply(Entity target, float progress);
}
