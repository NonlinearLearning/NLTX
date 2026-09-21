using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Reads committed jungle chest and loot-generation facts without external effects.
/// </summary>
public static class JungleChestAndLootGenerationQuery
{
  public static JungleChestAndLootGenerationSnapshot Snapshot(
    JungleChestAndLootGenerationStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}
