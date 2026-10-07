namespace Terraria.Network;

/// <summary>Detached packet 151 projection after an authoritative world-item removal.</summary>
public readonly record struct NetworkWorldItemDespawnProjection(short ItemIndex);
