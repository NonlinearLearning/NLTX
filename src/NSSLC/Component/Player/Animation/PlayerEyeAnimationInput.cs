namespace Terraria.Player.Animation;

public readonly record struct PlayerEyeAnimationInput(
  int CurrentLife,
  int MaximumLife,
  bool IsBlackout,
  bool IsBlind,
  bool IsSleeping,
  bool IsItemAnimating,
  int TimeSleeping,
  bool IsTipsy,
  bool IsPoisoned,
  bool IsVenomous,
  bool IsStarving,
  bool IsZoneSandstorm,
  bool IsZoneSnow,
  bool IsRaining,
  bool IsBehindBackWall)
{
  public bool CountsAsModeratelyDamaged =>
    (float)CurrentLife <= (float)MaximumLife * 0.25f;

  public bool HasPoisonCondition => IsPoisoned || IsVenomous || IsStarving;

  public bool HasStormExposure => IsZoneSandstorm || (IsZoneSnow && IsRaining);
}
