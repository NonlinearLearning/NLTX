using System.Collections.Generic;
using Terraria.Npc;

internal sealed class RecordingNpcSpawnSlotProtectionPort : INpcSpawnSlotProtectionPort
{
  private readonly bool[] _activeSlots;
  private readonly int[] _protectionTicks;

  public RecordingNpcSpawnSlotProtectionPort(
    bool[] activeSlots,
    int[] protectionTicks,
    List<string> calls)
  {
    _activeSlots = activeSlots;
    _protectionTicks = protectionTicks;
    Calls = calls;
  }

  public List<string> Calls { get; }

  public int SlotCount => _activeSlots.Length;

  public bool IsNpcActive(int slotIndex)
  {
    Calls.Add($"active:{slotIndex}");
    return _activeSlots[slotIndex];
  }

  public int ReadProtectionTicks(int slotIndex)
  {
    Calls.Add($"read:{slotIndex}");
    return _protectionTicks[slotIndex];
  }

  public void WriteProtectionTicks(int slotIndex, int protectionTicks)
  {
    Calls.Add($"write:{slotIndex}:{protectionTicks}");
    _protectionTicks[slotIndex] = protectionTicks;
  }
}
