using Terraria.EntityLifecycleAttribution;

namespace Terraria.Input.LockOn;

public sealed class LockOnInputSystem
{
  private readonly LockOnPolicy _policy;

  public LockOnInputSystem(LockOnPolicy policy)
  {
    _policy = policy;
  }

  public void Advance(LockOnSelectionComponent component, int ticks, bool inputHeld)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (ticks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ticks));
    }

    for (int tick = 0; tick < ticks; tick++)
    {
      if (!component.CanLockOn || !IsSelectedTargetValid(component))
      {
        component.ClearSelection();
        continue;
      }

      if (inputHeld)
      {
        component.SetSelection(
          component.SelectedCandidateIndex,
          _policy.HoldLifetimeTicks);
        continue;
      }

      int remaining = component.HoldTicksRemaining - 1;
      if (remaining <= 0)
      {
        component.ClearSelection();
      }
      else
      {
        component.SetSelection(component.SelectedCandidateIndex, remaining);
      }
    }
  }

  public void ReplaceCandidates(
    LockOnSelectionComponent component,
    IReadOnlyList<EntityReference> candidates)
  {
    ArgumentNullException.ThrowIfNull(component);
    ArgumentNullException.ThrowIfNull(candidates);
    var uniqueCandidates = new List<EntityReference>();
    var seen = new HashSet<EntityReference>();
    foreach (EntityReference candidate in candidates)
    {
      if (candidate.IsValid && seen.Add(candidate))
      {
        uniqueCandidates.Add(candidate);
      }
    }

    component.ReplaceCandidates(uniqueCandidates);
  }

  public void SetCanLockOn(LockOnSelectionComponent component, bool canLockOn)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.SetCanLockOn(canLockOn);
  }

  public bool TrySelect(LockOnSelectionComponent component, int candidateIndex)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (!component.CanLockOn || candidateIndex < 0 ||
      candidateIndex >= component.CandidateTargetReferences.Count ||
      !component.CandidateTargetReferences[candidateIndex].IsValid)
    {
      component.ClearSelection();
      return false;
    }

    component.SetSelection(candidateIndex, _policy.HoldLifetimeTicks);
    return true;
  }

  private static bool IsSelectedTargetValid(LockOnSelectionComponent component)
  {
    return LockOnTargetQuery.TryGetSelectedTarget(component, out _);
  }
}
