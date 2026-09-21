namespace Terraria.UiItemLocalization.Achievements;

public sealed record AchievementSaveSnapshot(
  int SchemaVersion,
  string PlayerPersistenceId,
  IReadOnlyList<AchievementConditionSaveSnapshot> Conditions)
{
  public static AchievementSaveSnapshot Create(
    string playerPersistenceId,
    IEnumerable<AchievementConditionSaveSnapshot> conditions)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(playerPersistenceId);
    ArgumentNullException.ThrowIfNull(conditions);
    return new AchievementSaveSnapshot(
      SchemaVersion: 1,
      playerPersistenceId,
      conditions.ToArray());
  }
}
