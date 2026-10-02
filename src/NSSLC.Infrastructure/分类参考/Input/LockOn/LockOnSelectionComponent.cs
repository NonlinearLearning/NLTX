using System.Collections.ObjectModel;
using Terraria.EntityLifecycleAttribution;

namespace Terraria.Input.LockOn;

public sealed class LockOnSelectionComponent
{
  private readonly List<EntityReference> _candidateTargetReferences = new();
  private readonly ReadOnlyCollection<EntityReference> _readOnlyCandidateTargetReferences;

  public LockOnSelectionComponent()
  {
    _readOnlyCandidateTargetReferences = _candidateTargetReferences.AsReadOnly();
    SelectedCandidateIndex = -1;
  }

  public IReadOnlyList<EntityReference> CandidateTargetReferences =>
    _readOnlyCandidateTargetReferences;

  public bool CanLockOn { get; private set; }

  public int HoldTicksRemaining { get; private set; }

  public int SelectedCandidateIndex { get; private set; }

  internal void ClearSelection()
  {
    SelectedCandidateIndex = -1;
    HoldTicksRemaining = 0;
  }

  internal void ReplaceCandidates(IReadOnlyList<EntityReference> candidates)
  {
    _candidateTargetReferences.Clear();
    _candidateTargetReferences.AddRange(candidates);
    if (SelectedCandidateIndex < 0 || SelectedCandidateIndex >= _candidateTargetReferences.Count)
    {
      ClearSelection();
    }
  }

  internal void SetCanLockOn(bool canLockOn)
  {
    CanLockOn = canLockOn;
    if (!canLockOn)
    {
      _candidateTargetReferences.Clear();
      ClearSelection();
    }
  }

  internal void SetSelection(int candidateIndex, int holdTicksRemaining)
  {
    SelectedCandidateIndex = candidateIndex;
    HoldTicksRemaining = holdTicksRemaining;
  }
}
