using System;

namespace Terraria.WorldFile.V319.Model;

public sealed record LegacyNpc(
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
}
