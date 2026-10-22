using System.Numerics;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>Unquantized server motion retained alongside the module-13 projection.</summary>
public sealed record LeashedEntityMotionState(
  Vector2 Position,
  Vector2 Velocity,
  int SpriteDirection,
  long LastTick,
  int? Direction = null);
