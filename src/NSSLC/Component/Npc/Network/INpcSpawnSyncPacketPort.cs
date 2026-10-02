namespace Terraria.Npc.Network;

public interface INpcSpawnSyncPacketPort
{
  /// <summary>
  /// Declares an NPC sync packet request for one legacy NPC slot.
  /// </summary>
  void SendNpcSyncPacket(int npcLegacySlot);
}
