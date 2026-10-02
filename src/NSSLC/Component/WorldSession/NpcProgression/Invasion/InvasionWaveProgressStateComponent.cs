using System;

namespace Terraria.WorldSession.NpcProgression.Invasion;

public sealed class InvasionWaveProgressStateComponent
{
  public float TotalInvasionPoints { get; private set; }

  public float WaveKills { get; private set; }

  public int WaveNumber { get; private set; }

  public void Commit(float totalInvasionPoints, float waveKills, int waveNumber)
  {
    ValidateFiniteNonNegative(totalInvasionPoints, nameof(totalInvasionPoints));
    ValidateFiniteNonNegative(waveKills, nameof(waveKills));
    ArgumentOutOfRangeException.ThrowIfNegative(waveNumber);

    if (waveNumber < WaveNumber)
    {
      throw new ArgumentOutOfRangeException(
        nameof(waveNumber),
        "Invasion wave number cannot move backwards without an explicit reset.");
    }

    TotalInvasionPoints = totalInvasionPoints;
    WaveKills = waveKills;
    WaveNumber = waveNumber;
  }

  public void Reset()
  {
    TotalInvasionPoints = 0.0f;
    WaveKills = 0.0f;
    WaveNumber = 0;
  }

  private static void ValidateFiniteNonNegative(float value, string parameterName)
  {
    if (!float.IsFinite(value) || value < 0.0f)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        "Invasion progress must be finite and non-negative.");
    }
  }
}
