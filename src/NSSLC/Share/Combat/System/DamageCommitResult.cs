namespace Terraria.Share.Combat;

/// <summary>Result of the sole target-life write boundary.</summary>
public enum DamageCommitResult : byte
{
  Applied = 0,
  TargetUnavailable = 1,
  TargetChanged = 2,
  NoDamage = 3,
  InvalidDamage = 4,
  SourceUnavailable = 5,
  SourceInactive = 6
}
