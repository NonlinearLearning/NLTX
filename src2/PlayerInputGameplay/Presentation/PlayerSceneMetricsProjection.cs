namespace NLTX.PlayerInputGameplay.Presentation;

public sealed class PlayerSceneMetricsProjection
{
  public SceneMetricsSnapshot Current { get; private set; }

  public void Set(SceneMetricsSnapshot snapshot)
  {
    if (snapshot.ActivePlayers < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(snapshot));
    }

    Current = snapshot;
  }
}
