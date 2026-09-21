namespace Terraria.WorldSession.Components;

/// <summary>
/// Represents the seven saved-tier fields in the Version4 world-state packet order.
/// </summary>
public readonly record struct WorldSavedOreTierNetworkFields(
  short Copper,
  short Iron,
  short Silver,
  short Gold,
  short Cobalt,
  short Mythril,
  short Adamantite);
