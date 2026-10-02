namespace Terraria.Server.Npc;

public struct NpcClientReplicationState
{
  public byte SkippedSyncCount;
  public uint LastAcknowledgedRevision;
}
