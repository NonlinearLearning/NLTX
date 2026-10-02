namespace NLTX.PlayerInputGameplay.Interaction;

public sealed class SmartInteractionCandidateBuffer
{
  private readonly List<SmartInteractionCandidateSnapshot> _candidates = new();

  public IReadOnlyList<SmartInteractionCandidateSnapshot> Candidates => _candidates;

  public void Add(SmartInteractionCandidateSnapshot candidate)
  {
    if (float.IsNaN(candidate.DistanceFromCursor) || float.IsInfinity(candidate.DistanceFromCursor) || candidate.DistanceFromCursor < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(candidate));
    }

    _candidates.Add(candidate);
  }

  public void Clear()
  {
    _candidates.Clear();
  }
}
