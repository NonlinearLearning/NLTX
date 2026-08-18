using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WorldRuleSnapshotComponent
{
  public WorldRuleSnapshotComponent(int difficulty, string secretSeedVariant, bool isHardmode)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(secretSeedVariant);
    if (difficulty < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(difficulty));
    }

    Difficulty = difficulty;
    SecretSeedVariant = secretSeedVariant;
    IsHardmode = isHardmode;
  }

  public int Difficulty { get; }
  public string SecretSeedVariant { get; }
  public bool IsHardmode { get; }
}
