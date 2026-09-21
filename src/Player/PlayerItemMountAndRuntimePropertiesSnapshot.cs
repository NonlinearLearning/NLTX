using System.Numerics;

namespace Terraria.Player;

public readonly record struct PlayerItemMountAndRuntimePropertiesSnapshot(
  bool HasMinionRestTarget,
  bool ItemTimeIsZero,
  bool ItemAnimationJustStarted,
  bool UsingOrReusingItem,
  PlayerSceneMetricsSnapshot SceneMetrics,
  Vector2 SpectatingCameraPosition,
  bool SlimeDontHyperJump,
  bool HasBreathingReed,
  bool IsRidingTracks,
  Vector2? MouthPosition,
  Vector2? HandPosition);
