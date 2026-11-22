using System;
using System.Collections.Generic;

namespace Terraria.Entity;

/// <summary>Owns remaining ticks for named cooldown families.</summary>
public sealed class CooldownComponent
{
  private readonly Dictionary<CooldownKind, int> _remaining = [];

  public int GetRemaining(CooldownKind kind)
  {
    Validate(kind);
    return _remaining.TryGetValue(kind, out int remaining) ? remaining : 0;
  }

  public bool IsReady(CooldownKind kind)
  {
    return GetRemaining(kind) == 0;
  }

  public void Start(CooldownKind kind, int durationTicks)
  {
    Validate(kind);
    if (durationTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(durationTicks));
    }

    if (durationTicks == 0)
    {
      _remaining.Remove(kind);
    }
    else
    {
      _remaining[kind] = durationTicks;
    }
  }

  public void AdvanceOneTick()
  {
    CooldownKind[] kinds = [.. _remaining.Keys];
    foreach (CooldownKind kind in kinds)
    {
      int next = _remaining[kind] - 1;
      if (next <= 0)
      {
        _remaining.Remove(kind);
      }
      else
      {
        _remaining[kind] = next;
      }
    }
  }

  private static void Validate(CooldownKind kind)
  {
    if (!Enum.IsDefined(kind))
    {
      throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown cooldown kind.");
    }
  }
}
