namespace Terraria.Player;

public readonly record struct PlayerActivityInput(
  bool IsConsideredStandingStill,
  bool IsItemAnimationActive,
  bool HeldItemIsKite,
  bool HasMovementInput,
  bool HasControlInput,
  bool HasAttackInput,
  bool HasMouseItem,
  bool IsPetting,
  bool IsSitting,
  bool IsSleeping,
  bool IsPaused,
  bool IsDead);
