using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct DungeonLayoutProviderSettingsSnapshot
{
  public DungeonLayoutProviderSettingsSnapshot(DungeonStyleLookupEntry style)
  {
    ArgumentNullException.ThrowIfNull(style);
    Style = style;
  }

  public DungeonStyleLookupEntry Style { get; }
}
