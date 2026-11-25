namespace Terraria.Share.Movement;

/// <summary>Declares whether an entity participates in gravity simulation.</summary>
public readonly record struct GravityCapabilityComponent(
    bool Enabled,
    bool CanToggleDirection);
