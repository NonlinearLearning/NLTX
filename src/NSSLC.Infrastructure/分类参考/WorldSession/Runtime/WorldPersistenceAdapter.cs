namespace Terraria.WorldSession.Runtime;

public sealed class WorldPersistenceAdapter
{
  private WorldSessionCommittedSnapshot? _lastCommittedSnapshot;

  public WorldSessionCommittedSnapshot? LastCommittedSnapshot => _lastCommittedSnapshot;

  public WorldPersistenceResult Commit(WorldSessionCommittedSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    _lastCommittedSnapshot = snapshot;
    return WorldPersistenceResult.Success;
  }

  public void Rollback()
  {
    _lastCommittedSnapshot = null;
  }
}
