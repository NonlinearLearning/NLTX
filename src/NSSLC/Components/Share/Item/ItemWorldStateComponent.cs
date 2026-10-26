using System;

namespace Terraria.Item;

/// <summary>
/// State needed only while an item stack is represented by a world entity.
/// Position and velocity remain owned by the shared spatial components.
/// </summary>
public sealed class ItemWorldStateComponent {
  public ItemWorldStateComponent(int pickupDelayTicks = 0, int? lifetimeTicks = null) {
    if (pickupDelayTicks < 0) {
      throw new ArgumentOutOfRangeException(nameof(pickupDelayTicks));
    }

    if (lifetimeTicks < 0) {
      throw new ArgumentOutOfRangeException(nameof(lifetimeTicks));
    }

    PickupDelayTicks = pickupDelayTicks;
    LifetimeTicks = lifetimeTicks;
  }

  public int PickupDelayTicks { get; private set; }

  /// <summary>Null means that this state has no time-based expiry.</summary>
  public int? LifetimeTicks { get; private set; }

  public bool CanBePickedUp => PickupDelayTicks == 0;

  public bool IsExpired => LifetimeTicks is 0;

  public void AdvanceTick() {
    if (PickupDelayTicks > 0) {
      PickupDelayTicks--;
    }

    if (LifetimeTicks is > 0) {
      LifetimeTicks--;
    }
  }

  public void SetPickupDelay(int ticks) {
    if (ticks < 0) {
      throw new ArgumentOutOfRangeException(nameof(ticks));
    }

    PickupDelayTicks = ticks;
  }
}
