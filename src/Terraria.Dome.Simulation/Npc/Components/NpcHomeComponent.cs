using System;

namespace Terraria.Dome.Simulation.Npc.Components;

public struct NpcHomeComponent
{
  public NpcHomeComponent(
    short homeTileX,
    short homeTileY,
    bool isHomeless,
    int returnTimeoutTicks,
    int townVariant = 0)
  {
    if (returnTimeoutTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(returnTimeoutTicks));
    }

    if (townVariant < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(townVariant));
    }

    HomeTileX = homeTileX;
    HomeTileY = homeTileY;
    IsHomeless = isHomeless;
    ReturnTimeoutTicks = returnTimeoutTicks;
    TownVariant = townVariant;
  }

  public short HomeTileX;
  public short HomeTileY;
  public bool IsHomeless;
  public int ReturnTimeoutTicks;
  public int TownVariant;
}
