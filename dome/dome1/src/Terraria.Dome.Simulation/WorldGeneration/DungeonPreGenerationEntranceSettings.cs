using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct DungeonPreGenerationEntranceSettings
{
  public DungeonPreGenerationEntranceSettings(
    DungeonEntranceGenerationSettings entranceSettings,
    int buriedEntranceYOffset,
    int buriedEntranceSandDugoutYOffset,
    int roughHeight,
    bool buryEntrance)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(buriedEntranceYOffset);
    ArgumentOutOfRangeException.ThrowIfNegative(buriedEntranceSandDugoutYOffset);
    ArgumentOutOfRangeException.ThrowIfNegative(roughHeight);
    EntranceSettings = entranceSettings;
    BuriedEntranceYOffset = buriedEntranceYOffset;
    BuriedEntranceSandDugoutYOffset = buriedEntranceSandDugoutYOffset;
    RoughHeight = roughHeight;
    BuryEntrance = buryEntrance;
  }

  public DungeonEntranceGenerationSettings EntranceSettings { get; }

  public int BuriedEntranceYOffset { get; }

  public int BuriedEntranceSandDugoutYOffset { get; }

  public int RoughHeight { get; }

  public bool BuryEntrance { get; }
}
