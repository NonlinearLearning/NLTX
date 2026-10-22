namespace MinimalImplementation.Server.Modules.Events;

/// <summary>
/// Ordered post-commit work; the Host retains results until their consumers settle.
/// </summary>
public abstract record InvasionEffect {
  // Seed the progress coordinator and start damage tracking; StartInvasion does not send packet78.
  public sealed record Started(
      int InvasionType, InvasionProgressEffect InitialProgress) : InvasionEffect;

  // The progression owner decides whether this is the first clear and schedules Lantern Night.
  public sealed record ClearEvent(
      int InvasionType, int GameEventId, short AchievementEventId) : InvasionEffect;

  public sealed record Warning(string LocalizationKey) : InvasionEffect;

  public sealed record SynchronizeWorldData : InvasionEffect;

  public sealed record ReportProgress(InvasionProgressEffect Progress) : InvasionEffect;
}
