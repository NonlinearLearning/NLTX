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

  public byte PaintColor => Paint;

  public byte PaintCoating => Dye;

  public bool PaintOrCoating => Paint != 0 || Dye != 0;

  public bool HasNameOverride => !string.IsNullOrEmpty(NameOverride);

  public void Validate()
  {
    if (NameOverride is not null && NameOverride.Length > MaximumNameOverrideLength)
    {
      throw new ArgumentOutOfRangeException(nameof(NameOverride));
    }
  }
}
