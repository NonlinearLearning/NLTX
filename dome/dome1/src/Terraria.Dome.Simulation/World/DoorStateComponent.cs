using System;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed class DoorStateComponent
{
  public DoorStateComponent(
    int doorId,
    int tileX,
    int tileY,
    bool isOpen = false,
    long revision = 1)
  {
    if (doorId <= 0 || revision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(doorId));
    }

    DoorId = doorId;
    TileX = tileX;
    TileY = tileY;
    IsOpen = isOpen;
    Revision = revision;
  }

  public int DoorId { get; }
  public bool IsOpen { get; private set; }
  public long Revision { get; private set; }
  public int TileX { get; private set; }
  public int TileY { get; private set; }

  public bool TrySetOpen(bool isOpen)
  {
    if (IsOpen == isOpen || Revision == long.MaxValue)
    {
      return false;
    }

    IsOpen = isOpen;
    Revision++;
    return true;
  }

  public bool TryApplyAuthorization(DoorAuthorizationPolicy authorization, bool requestedOpen)
  {
    if (!authorization.IsAllowed)
    {
      return false;
    }

    return TrySetOpen(requestedOpen);
  }

  public bool TryApplyLockPolicy(
    DoorLockPolicy lockPolicy,
    PlayerHandle? actor,
    bool requestedOpen)
  {
    return lockPolicy.Allows(actor) && TrySetOpen(requestedOpen);
  }

  public void MoveAnchor(int tileX, int tileY)
  {
    TileX = tileX;
    TileY = tileY;
  }
}
