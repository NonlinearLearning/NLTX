namespace Terraria.Npc.Network;

public interface INpcBuffSlotSyncPacketPort
{
  void SendBuffSlotSyncPacket(int npcLegacySlot);
}
