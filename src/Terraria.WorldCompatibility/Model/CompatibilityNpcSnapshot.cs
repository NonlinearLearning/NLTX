using System;
using Terraria.WorldFile.V319.Model;

namespace Terraria.WorldCompatibility.Model;

public sealed record CompatibilityNpcSnapshot(
  int Type,
  string Name,
  float PositionX,
  float PositionY,
  bool IsHomeless,
  int HomeX,
  int HomeY,
  string LegacyTypeName = "",
  bool IsTownNpc = true,
  int TownVariationIndex = 0,
  bool HomelessDespawn = false)
{
  public string LegacyTypeName { get; init; } = LegacyTypeName ??
    throw new ArgumentNullException(nameof(LegacyTypeName));

  public string Name { get; init; } = Name ?? throw new ArgumentNullException(nameof(Name));

  internal static CompatibilityNpcSnapshot From(LegacyNpc npc)
  {
    ArgumentNullException.ThrowIfNull(npc);
    return new CompatibilityNpcSnapshot(
      npc.Type,
      npc.Name,
      npc.PositionX,
      npc.PositionY,
      npc.IsHomeless,
      npc.HomeX,
      npc.HomeY,
      npc.LegacyTypeName,
      npc.IsTownNpc,
      npc.TownVariationIndex,
      npc.HomelessDespawn);
  }
}
