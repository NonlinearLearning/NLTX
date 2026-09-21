using System.Numerics;

namespace Terraria.Player;

public readonly record struct PlayerInteractionAndSelectionPropertiesInput(
  int Direction,
  float GravityDirection,
  int SelectedItem,
  IReadOnlyList<ItemEntityRef> Inventory,
  bool CanFloatInWater,
  bool ControlDown,
  bool MountActive,
  ContentId<MountDefinition>? MountType,
  bool Active,
  bool Dead,
  bool ShouldNotDraw,
  float Stealth,
  bool IsVoidVaultEnabled,
  Vector2 Position,
  Vector2? NetCameraTarget,
  bool ControlUp,
  bool TryKeepingHoveringUp,
  bool TryKeepingHoveringDown);
