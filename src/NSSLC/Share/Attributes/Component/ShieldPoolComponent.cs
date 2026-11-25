using System;

namespace Terraria.Share.Attributes;

/// <summary>
/// Owns the current shield resource for one entity.
/// </summary>
/// <remarks>
/// Shield capacity remains a calculated <see cref="AttributeKind.MaxShield"/> value. This
/// component stores only the mutable current pool, so consuming a shield cannot overwrite an
/// attribute base value or a capacity modifier.
/// </remarks>
public sealed class ShieldPoolComponent
{
  public ShieldPoolComponent(int current = 0)
  {
    if (current < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(current));
    }

    Current = current;
  }

  public int Current { get; private set; }

  /// <summary>Consumes up to <paramref name="requestedDamage"/> from the current pool.</summary>
  public int Consume(int requestedDamage)
  {
    if (requestedDamage < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(requestedDamage));
    }

    int consumed = Math.Min(Current, requestedDamage);
    Current -= consumed;
    return consumed;
  }

  /// <summary>Sets the current pool after a validated recharge or capacity synchronization.</summary>
  public void SetCurrent(int current)
  {
    if (current < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(current));
    }

    Current = current;
  }
}
