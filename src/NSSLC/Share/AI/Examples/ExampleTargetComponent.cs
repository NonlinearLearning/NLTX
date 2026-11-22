using System.Numerics;

namespace Terraria.Ai.Examples;

/// <summary>
/// Example target observation. The host changes Revision when the selected target changes.
/// Revision is an observation version, not an ECS identity; target lifetime stays with the host.
/// </summary>
public readonly record struct ExampleTargetComponent(
    Vector2 Position, bool IsVisible, bool IsAlive, int Revision);
