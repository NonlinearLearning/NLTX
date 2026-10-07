namespace Terraria.WorldSession.Components;

/// <summary>
/// Holds saved-tier values in the legacy world-file write order.
/// </summary>
public readonly record struct WorldSavedOreTierFileSaveValues(
  int Cobalt,
  int Mythril,
  int Adamantite,
  int Copper,
  int Iron,
  int Silver,
  int Gold);
