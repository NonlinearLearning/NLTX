using System;

namespace EntityEcs.Components;

// status: proposed
public sealed class EntityLifecycleState
{
  public EntityLifecycleState(
    LifecyclePhase phase = LifecyclePhase.Uninitialized,
    bool isActive = false,
    TerminationReason terminationReason = TerminationReason.None,
    bool pendingCleanup = false,
    long revision = 0)
  {
    if (revision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(revision));
    }

    Phase = phase;
    IsActive = isActive;
    TerminationReason = terminationReason;
    PendingCleanup = pendingCleanup;
    Revision = revision;
    Validate();
  }

  public LifecyclePhase Phase { get; }

  public bool IsActive { get; }

  public TerminationReason TerminationReason { get; }

  public bool PendingCleanup { get; }

  public long Revision { get; }

  public void Validate()
  {
    if (Phase == LifecyclePhase.Active && !IsActive)
    {
      throw new InvalidOperationException(
        "An active entity must report IsActive = true.");
    }

    if (Phase != LifecyclePhase.Active && IsActive)
    {
      throw new InvalidOperationException(
        "Only the Active phase may report IsActive = true.");
    }

    if ((Phase == LifecyclePhase.Ending || Phase == LifecyclePhase.Retired)
      && TerminationReason == TerminationReason.None)
    {
      throw new InvalidOperationException(
        "Ending and Retired entities require a termination reason.");
    }

    if (PendingCleanup
      && Phase != LifecyclePhase.Ending
      && Phase != LifecyclePhase.Retired)
    {
      throw new InvalidOperationException(
        "Pending cleanup is only valid after termination has started.");
    }
  }
}
