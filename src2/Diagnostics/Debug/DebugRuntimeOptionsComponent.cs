namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class DebugRuntimeOptionsComponent
{
  private DebugRuntimeOptionsSnapshot _snapshot;

  public void Apply(DebugRuntimeOptionsSnapshot snapshot)
  {
    if (snapshot.UpdateWaitInMs < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(snapshot));
    }

    _snapshot = snapshot;
  }

  public DebugRuntimeOptionsSnapshot Snapshot()
  {
    return _snapshot;
  }
}
