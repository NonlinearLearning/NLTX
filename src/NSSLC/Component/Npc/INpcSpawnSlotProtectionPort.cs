namespace Terraria.Npc;

public interface INpcSpawnSlotProtectionPort
{
  int SlotCount { get; }

  bool IsNpcActive(int slotIndex);

  int ReadProtectionTicks(int slotIndex);

  void WriteProtectionTicks(int slotIndex, int protectionTicks);
}
