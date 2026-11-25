using Terraria.Share.Relationships;

namespace Terraria.Share.Movement;

/// <summary>One movement intent a producing domain asked the Movement domain to install.</summary>
/// <remarks>
/// The producing domain decides <see cref="MovementIntent"/> — including its
/// <see cref="MovementIntentSource"/> and priority — but does not write the component. Movement
/// installs it, so the component has one writer per domain boundary instead of one per producer.
/// </remarks>
public readonly record struct MovementIntentRequest(
    EntityRuntimeHandle Target,
    MovementIntent Intent);
