namespace Terraria.WorldSession.Queries;

public sealed class AchievementServiceAdapter
{
  private readonly HashSet<string> _unlocked = new(StringComparer.Ordinal);

  public void Unlock(string achievementId)
  {
    if (string.IsNullOrWhiteSpace(achievementId))
    {
      throw new ArgumentException("An achievement ID is required.", nameof(achievementId));
    }

    _unlocked.Add(achievementId);
  }

  public bool IsUnlocked(string achievementId)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(achievementId);
    return _unlocked.Contains(achievementId);
  }
}
