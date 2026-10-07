using System;

namespace Terraria.Town.Doors;

/// <summary>
/// 保存城镇 NPC 待执行的门交互意图。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：closeDoor（第 6425 行）； doorX（第 6427 行）； doorY（第 6429 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-p13-npc-town-progression-component-design.md。</para>
/// <para>依据位置：第 141 行。</para>
/// </remarks>
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
