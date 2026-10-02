using Terraria.WorldGeneration.Host;

namespace Terraria.WorldGeneration.Projections;

public readonly record struct WorldGenerationProgressProjection
{
  public WorldGenerationProgressProjection(
    uint updateCount,
    string passId,
    string? message,
    float fraction,
    WorldGenerationHostPhase phase)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(passId);
    if (!float.IsFinite(fraction) || fraction < 0f || fraction > 1f)
    {
      throw new ArgumentOutOfRangeException(nameof(fraction));
    }

    UpdateCount = updateCount;
    PassId = passId;
    Message = message;
    Fraction = fraction;
    Phase = phase;
  }

  public uint UpdateCount { get; }

  public string PassId { get; }

  public string? Message { get; }

  public float Fraction { get; }

  public WorldGenerationHostPhase Phase { get; }
}
