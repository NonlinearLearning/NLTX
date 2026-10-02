namespace Terraria.Player;

public sealed class PlayerAchievementEligibilityStateComponent
{
  public int FramesLeftEligibleForDeadmansChestDeathAchievement { get; internal set; }

  internal void ResetForLifecycle()
  {
    FramesLeftEligibleForDeadmansChestDeathAchievement = 0;
  }
}
