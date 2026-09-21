namespace Terraria.WorldSession.Runtime;

public readonly record struct RuleOverrideSnapshot
{
  public float? Difficulty { get; }

  public RuleOverrideSnapshot(float? difficulty)
  {
    Difficulty = Validate(difficulty);
  }

  private static float? Validate(float? difficulty)
  {
    if (difficulty.HasValue &&
      (float.IsNaN(difficulty.Value) || float.IsInfinity(difficulty.Value)))
    {
      throw new ArgumentOutOfRangeException(nameof(difficulty));
    }

    return difficulty;
  }
}
