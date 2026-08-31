using System;

namespace Terraria.Dome.Simulation.Items.Components;

public readonly record struct ItemInstanceStateComponent(
  ushort PrefixId = 0,
  ushort VariantId = 0,
  byte Dye = 0,
  byte Paint = 0,
  bool IsFavorited = false,
  bool IsNewAndShiny = false,
  string? NameOverride = null)
{
  public const int MaximumNameOverrideLength = 200;

  public void Validate()
  {
    if (NameOverride is not null && NameOverride.Length > MaximumNameOverrideLength)
    {
      throw new ArgumentOutOfRangeException(nameof(NameOverride));
    }
  }
}
