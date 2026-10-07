using System;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Schedules background world transformation work and main-thread follow-up effects.
/// </summary>
public interface IWorldTransformationScheduler
{
  void ScheduleBackground(Action work);

  void QueueMainThread(Action work);
}
