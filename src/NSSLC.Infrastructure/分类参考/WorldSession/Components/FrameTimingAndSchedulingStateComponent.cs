namespace Terraria.WorldSession.Components;

public sealed class FrameTimingAndSchedulingStateComponent
{
  public float WrappedHour { get; internal set; }

  public bool GlobalTimerPaused { get; internal set; }

  public DedicatedServerFrameMetrics Metrics { get; } = new();
}
