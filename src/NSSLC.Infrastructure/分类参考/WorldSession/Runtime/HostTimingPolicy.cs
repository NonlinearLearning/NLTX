namespace Terraria.WorldSession.Runtime;

public readonly record struct HostTimingPolicy(
  TimeSpan InactiveSleepTime,
  bool IsFixedTimeStep,
  TimeSpan TargetElapsedTime);
