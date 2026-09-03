using System;

namespace Terraria.Dome.Simulation.Player.Components;

public sealed class PlayerInventoryTransferPolicyComponent
{
  public const int SlotCount = 59;
  private readonly bool[] _chestStack = new bool[SlotCount];

  public ReadOnlySpan<bool> ChestStack => _chestStack;

  public bool Get(int slot)
  {
    ValidateSlot(slot);
    return _chestStack[slot];
  }

  public void Set(int slot, bool enabled)
  {
    ValidateSlot(slot);
    _chestStack[slot] = enabled;
  }

  public void Clear()
  {
    Array.Clear(_chestStack);
  }

  private static void ValidateSlot(int slot)
  {
    if (slot < 0 || slot >= SlotCount)
    {
      throw new ArgumentOutOfRangeException(nameof(slot));
    }
  }
}
