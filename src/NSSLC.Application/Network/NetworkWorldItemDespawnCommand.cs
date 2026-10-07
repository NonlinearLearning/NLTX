namespace Terraria.Network;

/// <summary>Authenticated request to remove one currently registered world-item slot.</summary>
public readonly record struct NetworkWorldItemDespawnCommand(short ItemIndex);
