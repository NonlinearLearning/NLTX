namespace Terraria.Items.InventoryContainers;

public sealed class WorldItemSceneMetricsAdapter : IWorldItemSceneMetricsPort
{
  private readonly Func<WorldItemSceneMetricsSnapshot> _read;

  public WorldItemSceneMetricsAdapter(Func<WorldItemSceneMetricsSnapshot> read)
  {
    _read = read ?? throw new ArgumentNullException(nameof(read));
  }

  public WorldItemSceneMetricsSnapshot Read()
  {
    WorldItemSceneMetricsSnapshot snapshot = _read.Invoke();
    if (!snapshot.IsValid)
    {
      throw new InvalidOperationException("Scene metrics returned an invalid player count.");
    }

    return snapshot;
  }
}
