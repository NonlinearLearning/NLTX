namespace Terraria.Player;

public readonly record struct PlayerTickCoordinatorResult(
  PlayerEquipmentProjectionResult Projection,
  PlayerEquipmentEffectSnapshot Effects,
  PlayerStatusEffectTickResult Buffs,
  PlayerDefenseInteractionResult Defense)
{
  // Resource state is optional for the legacy overload. When present, it is
  // the snapshot produced by the same tick composition, not a second owner.
  public PlayerBuffResourceSnapshot? Resources { get; init; }

  public PlayerBuffResourceRebuildResult? ResourceRebuild { get; init; }
}
