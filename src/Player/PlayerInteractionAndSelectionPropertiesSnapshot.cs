using System.Numerics;

namespace Terraria.Player;

public readonly record struct PlayerInteractionAndSelectionPropertiesSnapshot(
  Vector2 Directions,
  int SelectedItem,
  ItemEntityRef HeldItem,
  bool ShouldFloatInWater,
  bool CanBeTalkedTo,
  bool IsVoidVaultEnabled,
  Vector2 ReportedCameraPosition,
  bool TryingToHoverUp,
  bool TryingToHoverDown);
