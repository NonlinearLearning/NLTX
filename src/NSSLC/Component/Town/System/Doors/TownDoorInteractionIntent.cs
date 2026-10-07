using System;

namespace Terraria.Town.Doors;

public readonly record struct TownDoorInteractionIntent
{
  public TownDoorInteractionIntent(
    bool closeDoor,
    int doorX,
    int doorY,
    long sequence,
    long expiresAtTick)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(doorX);
    ArgumentOutOfRangeException.ThrowIfNegative(doorY);
    ArgumentOutOfRangeException.ThrowIfNegative(sequence);
    ArgumentOutOfRangeException.ThrowIfNegative(expiresAtTick);

    CloseDoor = closeDoor;
    DoorX = doorX;
    DoorY = doorY;
    Sequence = sequence;
    ExpiresAtTick = expiresAtTick;
  }

  public bool CloseDoor { get; }

  public int DoorX { get; }

  public int DoorY { get; }

  public long Sequence { get; }

  public long ExpiresAtTick { get; }
}
