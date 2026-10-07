namespace Terraria.Npc;

public interface INpcSpawnSlotAcquisitionPort :
  INpcSpawnSlotProtectionPort,
  INpcSpawnTypeResolutionRandomPort
{
  NpcTypeId FromNetId(NpcTypeId resolvedType);

  bool SearchSpawnSlotsInReverse(NpcTypeId netType);

  bool CannotSpawnInSlot0(NpcTypeId netType);

  NpcSpawnSlotFact[] CaptureSlotFacts();
}
