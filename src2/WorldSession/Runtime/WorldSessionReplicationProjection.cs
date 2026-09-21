namespace Terraria.WorldSession.Runtime;

public sealed class WorldSessionReplicationProjection
{
  public WorldSessionCommittedSnapshot? Create(WorldSessionCommittedSnapshot? snapshot)
  {
    return snapshot;
  }
}
