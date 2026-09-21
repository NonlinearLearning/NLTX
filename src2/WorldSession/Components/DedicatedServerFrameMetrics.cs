namespace Terraria.WorldSession.Components;

public sealed class DedicatedServerFrameMetrics
{
  public int FramesPerSecond { get; internal set; }

  public int Count1 { get; internal set; }

  public int Count2 { get; internal set; }

  public int UpdatesInCurrentWindow { get; internal set; }

  public void ResetWindow()
  {
    UpdatesInCurrentWindow = 0;
  }
}
