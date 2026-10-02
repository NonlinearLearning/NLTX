using System.Numerics;

namespace Terraria.Player.Mount;

public readonly record struct MountFrameUpdateInput(
  MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind State,
  Vector2 Velocity,
  bool IsDisplayDollOrInanimate,
  bool JustJumped,
  bool ControlDown);

public readonly record struct MountFrameUpdateResult(
  bool Updated,
  MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind State,
  int Frame,
  int WalkingGraceRemainingTicks);
