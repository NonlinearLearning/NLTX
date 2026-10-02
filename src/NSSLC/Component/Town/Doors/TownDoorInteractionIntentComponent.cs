using System;

namespace Terraria.Town.Doors;

public sealed class TownDoorInteractionIntentComponent
{
  private TownDoorInteractionIntent? _pendingIntent;
  private long _nextSequence;

  public bool HasPendingIntent => _pendingIntent.HasValue;

  public bool CloseDoor => _pendingIntent?.CloseDoor ?? false;

  public int DoorX => _pendingIntent?.DoorX ?? 0;

  public int DoorY => _pendingIntent?.DoorY ?? 0;

  public long PendingSequence => _pendingIntent?.Sequence ?? 0;

  public long PendingExpiresAtTick => _pendingIntent?.ExpiresAtTick ?? 0;

  public TownDoorInteractionIntent Request(
    bool closeDoor,
    int doorX,
    int doorY,
    long expiresAtTick)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(doorX);
    ArgumentOutOfRangeException.ThrowIfNegative(doorY);
    ArgumentOutOfRangeException.ThrowIfNegative(expiresAtTick);

    _nextSequence = checked(_nextSequence + 1);
    TownDoorInteractionIntent intent = new(
      closeDoor,
      doorX,
      doorY,
      _nextSequence,
      expiresAtTick);
    _pendingIntent = intent;
    return intent;
  }

  public bool TryConsume(long currentTick, out TownDoorInteractionIntent intent)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(currentTick);
    if (!_pendingIntent.HasValue)
    {
      intent = default;
      return false;
    }

    TownDoorInteractionIntent pendingIntent = _pendingIntent.Value;
    if (currentTick >= pendingIntent.ExpiresAtTick)
    {
      _pendingIntent = null;
      intent = default;
      return false;
    }

    _pendingIntent = null;
    intent = pendingIntent;
    return true;
  }

  public bool ExpireIfNeeded(long currentTick)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(currentTick);
    if (!_pendingIntent.HasValue || currentTick < _pendingIntent.Value.ExpiresAtTick)
    {
      return false;
    }

    _pendingIntent = null;
    return true;
  }

  public bool Reject()
  {
    if (!_pendingIntent.HasValue)
    {
      return false;
    }

    _pendingIntent = null;
    return true;
  }

  public void Reset()
  {
    _pendingIntent = null;
    _nextSequence = 0;
  }
}
