using System.Numerics;

namespace Terraria.Share.Ai.Examples;

/// <summary>
/// Example value projection; the host reads the real movement/physics components.
/// </summary>
public readonly record struct ExampleMovementComponent(
    Vector2 Position, bool IsGrounded, bool JumpReady);
