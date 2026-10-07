using System;

namespace Terraria.Npc;

public static class NpcSpawnSlotProtectionSystem
{
  private const int ActiveProtectionTicks = 2;
  private const int NoProtectionTicks = 0;

  /// <summary>
  /// Recomputes each slot in order. Active slots do not read their previous protection value.
  /// </summary>
  public static void AdvanceTick(INpcSpawnSlotProtectionPort port)
  {
    ArgumentNullException.ThrowIfNull(port);

    for (int slotIndex = 0; slotIndex < port.SlotCount; slotIndex++)
    {
      bool isNpcActive = port.IsNpcActive(slotIndex);
      int protectionTicks = isNpcActive
        ? ActiveProtectionTicks
        : Math.Max(port.ReadProtectionTicks(slotIndex) - 1, NoProtectionTicks);

      port.WriteProtectionTicks(slotIndex, protectionTicks);
    }
  }

  /// <summary>
  /// Protects a slot immediately after selection and before NPC replacement or initialization.
  /// </summary>
  public static void ProtectSelectedSlot(
    int slotIndex,
    INpcSpawnSlotProtectionPort port)
  {
    ArgumentNullException.ThrowIfNull(port);
    port.WriteProtectionTicks(slotIndex, ActiveProtectionTicks);
  }

  /// <summary>
  /// Clears protection after the caller initializes the corresponding NPC slot.
  /// </summary>
  public static void ResetSlotProtection(
    int slotIndex,
    INpcSpawnSlotProtectionPort port)
  {
    ArgumentNullException.ThrowIfNull(port);
    port.WriteProtectionTicks(slotIndex, NoProtectionTicks);
  }
}
