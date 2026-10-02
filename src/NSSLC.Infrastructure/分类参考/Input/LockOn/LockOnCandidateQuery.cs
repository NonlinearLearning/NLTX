using System.Numerics;
using Terraria.EntityLifecycleAttribution;

namespace Terraria.Input.LockOn;

public static class LockOnCandidateQuery
{
  public readonly record struct Candidate(
    EntityReference TargetReference,
    Vector2 Position,
    bool IsTargetable);

  public static IReadOnlyList<EntityReference> Find(
    Vector2 origin,
    IReadOnlyList<Candidate> candidates,
    LockOnPolicy policy)
  {
    ArgumentNullException.ThrowIfNull(candidates);
    var selected = new List<EntityReference>();
    var seen = new HashSet<EntityReference>();
    foreach (Candidate candidate in candidates)
    {
      if (!candidate.IsTargetable || !candidate.TargetReference.IsValid ||
        Vector2.Distance(origin, candidate.Position) > policy.RangePixels ||
        !seen.Add(candidate.TargetReference))
      {
        continue;
      }

      selected.Add(candidate.TargetReference);
    }

    return Array.AsReadOnly(selected.ToArray());
  }
}
