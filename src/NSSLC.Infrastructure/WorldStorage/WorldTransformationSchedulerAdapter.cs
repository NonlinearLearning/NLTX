using System;
using System.Threading.Tasks;
using Terraria.NonAuthoritative.Persistence;

namespace Terraria.NonAuthoritative.WorldStorage;

/// <summary>
/// Uses the .NET task scheduler for transform work and a host callback for main-thread effects.
/// </summary>
public sealed class WorldTransformationSchedulerAdapter : IWorldTransformationScheduler
{
  private readonly Action<Action> _queueMainThreadAction;

  public WorldTransformationSchedulerAdapter(Action<Action> queueMainThreadAction)
  {
    _queueMainThreadAction = queueMainThreadAction ??
      throw new ArgumentNullException(nameof(queueMainThreadAction));
  }

  public void ScheduleBackground(Action work)
  {
    ArgumentNullException.ThrowIfNull(work);
    _ = Task.Run(work);
  }

  public void QueueMainThread(Action work)
  {
    ArgumentNullException.ThrowIfNull(work);
    _queueMainThreadAction.Invoke(work);
  }
}
