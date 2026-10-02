namespace Terraria.WorldSession.Runtime;

public sealed class DeferredProcessSchedulerPort
{
  private readonly List<QueuedProcess> _queued = new();

  public int Count => _queued.Count;

  public void Enqueue(
    Func<FrameScope, DeferredProcessResult> process,
    DeferredProcessLifetime lifetime)
  {
    ArgumentNullException.ThrowIfNull(process);
    _queued.Add(new QueuedProcess(process, lifetime));
  }

  public DeferredProcessDrainResult Drain(FrameScope scope)
  {
    var result = new DeferredProcessDrainResult();
    for (int i = _queued.Count - 1; i >= 0; i--)
    {
      QueuedProcess queued = _queued[i];
      if (!CanRun(queued.Lifetime, scope))
      {
        continue;
      }

      DeferredProcessResult processResult = queued.Process(scope);
      switch (processResult)
      {
        case DeferredProcessResult.Completed:
          _queued.RemoveAt(i);
          result = result with { Executed = result.Executed + 1 };
          break;
        case DeferredProcessResult.KeepQueued:
          result = result with { Retained = result.Retained + 1 };
          break;
        case DeferredProcessResult.Cancelled:
          _queued.RemoveAt(i);
          result = result with { Cancelled = result.Cancelled + 1 };
          break;
        case DeferredProcessResult.Failed:
          _queued.RemoveAt(i);
          result = result with { Failed = result.Failed + 1 };
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    return result;
  }

  public void Clear()
  {
    _queued.Clear();
  }

  private static bool CanRun(DeferredProcessLifetime lifetime, FrameScope scope)
  {
    return lifetime switch
    {
      DeferredProcessLifetime.Frame => scope == FrameScope.Frame,
      DeferredProcessLifetime.Session => scope is FrameScope.Frame or FrameScope.Session,
      DeferredProcessLifetime.Process => true,
      _ => throw new ArgumentOutOfRangeException(nameof(lifetime))
    };
  }

  private sealed record QueuedProcess(
    Func<FrameScope, DeferredProcessResult> Process,
    DeferredProcessLifetime Lifetime);
}
