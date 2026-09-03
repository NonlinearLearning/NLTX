using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerEnvelope(
  int MinX,
  int MaxXExclusive,
  int MinY,
  int MaxYExclusive,
  double Strength,
  double NextCenterX,
  double NextCenterY,
  double RemainingSteps);

public static class LegacyTileRunnerEnvelopePolicy
{
  public static LegacyTileRunnerEnvelope Advance(
    double centerX,
    double centerY,
    double initialStrength,
    int totalSteps,
    double remainingSteps,
    double speedX,
    double speedY,
    int worldWidth,
    int worldHeight)
  {
    if (!double.IsFinite(centerX) || !double.IsFinite(centerY) ||
        !double.IsFinite(initialStrength) || initialStrength <= 0 || totalSteps <= 0 ||
        !double.IsFinite(remainingSteps) || remainingSteps <= 0 ||
        !double.IsFinite(speedX) || !double.IsFinite(speedY) ||
        worldWidth < 2 || worldHeight < 2)
    {
      throw new ArgumentOutOfRangeException(nameof(centerX));
    }

    double strength = initialStrength * (remainingSteps / totalSteps);
    double nextRemainingSteps = remainingSteps - 1.0;
    int minX = Math.Max(1, (int)(centerX - strength * 0.5));
    int maxXExclusive = Math.Min(worldWidth - 1, (int)(centerX + strength * 0.5));
    int minY = Math.Max(1, (int)(centerY - strength * 0.5));
    int maxYExclusive = Math.Min(worldHeight - 1, (int)(centerY + strength * 0.5));
    return new LegacyTileRunnerEnvelope(
      minX,
      maxXExclusive,
      minY,
      maxYExclusive,
      strength,
      centerX + speedX,
      centerY + speedY,
      nextRemainingSteps);
  }
}
