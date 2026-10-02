using System.Numerics;

namespace Terraria.SpatialMotionPhysics;

public static class TeleportCandidateQuery
{
  public static TeleportCandidateResult Find(
    RandomTeleportationAttemptInput input,
    IReadOnlyList<Vector2> candidates,
    Func<Vector2, bool> isValid)
  {
    ArgumentNullException.ThrowIfNull(input);
    ArgumentNullException.ThrowIfNull(candidates);
    ArgumentNullException.ThrowIfNull(isValid);

    int attemptLimit = Math.Min(input.AttemptsBeforeGivingUp, candidates.Count);
    for (int index = 0; index < attemptLimit; index++)
    {
      Vector2 candidate = candidates[index];
      if (isValid(candidate) &&
        (input.SpecializedConditions is null ||
          input.SpecializedConditions.Invoke(candidate)))
      {
        return new TeleportCandidateResult(true, candidate);
      }
    }

    return new TeleportCandidateResult(false, Vector2.Zero);
  }
}
