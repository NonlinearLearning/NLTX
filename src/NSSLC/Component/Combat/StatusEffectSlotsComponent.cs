using System;
using System.Collections.Generic;

namespace Terraria.Combat;

public struct StatusEffectSlotsComponent
{
  public StatusEffectSlotsComponent(
    IReadOnlyList<StatusEffectSlot>? slots = null,
    int revision = 0)
  {
    _slots = slots is null
      ? null
      : new List<StatusEffectSlot>(slots).ToArray();
    Revision = revision;
  }

  private StatusEffectSlot[]? _slots;

  public int Revision;

  public ReadOnlyMemory<StatusEffectSlot> Slots =>
    new(_slots ?? Array.Empty<StatusEffectSlot>());

  public int Capacity => _slots?.Length ?? 0;
}
