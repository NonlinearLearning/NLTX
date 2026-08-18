namespace Terraria.Dome.Simulation.Npc.Components;

public struct NpcReplicationComponent
{
  public NpcReplicationComponent(int replicationId, long revision)
  {
    ReplicationId = replicationId;
    Revision = revision;
    IsDirty = true;
    ThrottleTicks = 0;
  }

  public int ReplicationId;
  public long Revision;
  public bool IsDirty;
  public int ThrottleTicks;
}
