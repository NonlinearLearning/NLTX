using System;

namespace Terraria.Dome.Simulation.Player.Components;

public sealed class PlayerGrappleStateComponent
{
  public const int SlotCount = 20;
  public const int InvalidTarget = -1;

  private readonly int[] _targets = new int[SlotCount];

  public PlayerGrappleStateComponent()
  {
    Clear();
  }

  public int Count { get; private set; }

  public ReadOnlySpan<int> Targets => _targets;

  public bool TryAttach(int target)
  {
    if (target < 0 || Count >= SlotCount)
    {
      return false;
    }

    _targets[Count] = target;
    Count++;
    return true;
  }

  public bool Remove(int target)
  {
    for (int index = 0; index < Count; index++)
    {
      if (_targets[index] != target)
      {
        continue;
      }

      Count--;
      _targets[index] = _targets[Count];
      _targets[Count] = InvalidTarget;
      return true;
    }

    return false;
  }

  public void Clear()
  {
    Array.Fill(_targets, InvalidTarget);
    Count = 0;
  }
}
