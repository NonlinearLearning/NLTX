using System;

namespace Terraria.WorldGeneration.Definitions;

public sealed class WorldGenerationConfigurationDefinition
{
  private WorldGenerationConfigurationDefinition(
    int worldWidth,
    int worldHeight,
    string seedText,
    int rulesVersion)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(worldWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(worldHeight);
    ArgumentException.ThrowIfNullOrWhiteSpace(seedText);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rulesVersion);

    WorldWidth = worldWidth;
    WorldHeight = worldHeight;
    SeedText = seedText;
    RulesVersion = rulesVersion;
  }

  public static WorldGenerationConfigurationDefinition Create(
    int worldWidth,
    int worldHeight,
    string seedText,
    int rulesVersion = 1)
  {
    return new WorldGenerationConfigurationDefinition(
      worldWidth,
      worldHeight,
      seedText,
      rulesVersion);
  }

  public int WorldWidth { get; }

  public int WorldHeight { get; }

  public string SeedText { get; }

  public int RulesVersion { get; }

  public WorldGenerationConfigurationSnapshot CreateSnapshot()
  {
    return new WorldGenerationConfigurationSnapshot(
      WorldWidth,
      WorldHeight,
      SeedText,
      RulesVersion);
  }
}
