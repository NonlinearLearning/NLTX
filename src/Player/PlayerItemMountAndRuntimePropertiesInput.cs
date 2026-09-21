using System.Numerics;

namespace Terraria.Player;

public readonly record struct PlayerItemMountAndRuntimePropertiesInput(
  Vector2 MinionRestTargetPoint,
  int ItemTime,
  int ItemAnimation,
  int ItemAnimationMax,
  int ReuseDelay,
  bool IsChanneling,
  bool HasPendingItemReuse,
  PlayerSceneMetricsSnapshot SceneMetrics,
  Vector2 Position,
  PlayerSpectatingCameraTargetSnapshot? SpectatingCameraTarget,
  bool MountActive,
  bool MountIsSlime,
  int WetSlime,
  bool ControlJump,
  int SelectedItemTypeId,
  bool MountAllowsHeldItems,
  bool MountIsCart,
  bool MountCanGrindRails,
  bool OnTrack,
  Vector2? MouthPositionOverride,
  Vector2? MouthPositionFallback,
  Vector2? HandPositionOverride,
  Vector2? HandPositionFallback);
