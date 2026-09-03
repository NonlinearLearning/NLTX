using System;
using Terraria.Dome.Simulation.Npc.Definitions;

namespace Terraria.Dome.Simulation.Npc.Components;

public struct NpcHomeComponent
{
  public const int KickOutLookForHomeTimeout = NpcLegacyFieldPolicy.KickOutLookForHomeTimeout;

  public NpcHomeComponent(
    short homeTileX,
    short homeTileY,
    bool isHomeless,
    int returnTimeoutTicks,
    int townVariant = 0,
    int lookForHomeTimeout = 0)
  {
    if (returnTimeoutTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(returnTimeoutTicks));
    }

    if (townVariant < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(townVariant));
    }

    if (lookForHomeTimeout < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(lookForHomeTimeout));
    }

    HomeTileX = homeTileX;
    HomeTileY = homeTileY;
    IsHomeless = isHomeless;
    ReturnTimeoutTicks = returnTimeoutTicks;
    TownVariant = townVariant;
    LookForHomeTimeout = lookForHomeTimeout;
  }

  public short HomeTileX;
  public short HomeTileY;
  public bool IsHomeless;
  public int ReturnTimeoutTicks;
  public int TownVariant;
  public int LookForHomeTimeout;

  public bool IsReadyToLookForHome => IsHomeless && LookForHomeTimeout == 0;

  public void MarkKickedOut()
  {
    IsHomeless = true;
    LookForHomeTimeout = KickOutLookForHomeTimeout;
  }

  public void MarkMovedRoom()
  {
    IsHomeless = true;
    LookForHomeTimeout = 0;
  }
}
