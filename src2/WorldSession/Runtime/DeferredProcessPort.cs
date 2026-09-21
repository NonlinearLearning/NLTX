namespace Terraria.WorldSession.Runtime;

public sealed class DeferredProcessPort
{
  private readonly DeferredProcessSchedulerPort _scheduler = new();

  public void Enqueue(
    Func<FrameScope, DeferredProcessResult> process,
    DeferredProcessLifetime lifetime)
  {
    _scheduler.Enqueue(process, lifetime);
  }

  public DeferredProcessDrainResult Drain(FrameScope scope)
  {
    return _scheduler.Drain(scope);
  }
}
