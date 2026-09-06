namespace Terraria.Combat;

public readonly record struct DamageResult(
  bool Applied,
  DamageRejectionReason RejectionReason,
  int FinalDamage,
  bool Killed,
  DamageAttribution Attribution);
