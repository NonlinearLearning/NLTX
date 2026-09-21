using System.Numerics;

namespace Terraria.Player;

public readonly record struct PlayerInstantMovementAccumulatorInput(
  Vector2 MovementDelta);
