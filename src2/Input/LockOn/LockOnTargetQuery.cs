using Terraria.EntityLifecycleAttribution;

namespace Terraria.Input.LockOn;

public static class LockOnTargetQuery
{
  public static bool TryGetSelectedTarget(
    LockOnSelectionComponent component,
    out EntityReference targetReference)
  {
    ArgumentNullException.ThrowIfNull(component);
    int selectedIndex = component.SelectedCandidateIndex;
    if (!component.CanLockOn || selectedIndex < 0 ||
      selectedIndex >= component.CandidateTargetReferences.Count)
    {
      targetReference = default;
      return false;
    }

    EntityReference candidate = component.CandidateTargetReferences[selectedIndex];
    if (!candidate.IsValid)
    {
      targetReference = default;
      return false;
    }

    targetReference = candidate;
    return true;
  }
}
