namespace Terraria.WorldSession.Components;

/// <summary>
/// Carries the successful post-altar ore selection from the progression boundary.
/// </summary>
public readonly record struct WorldSavedOreTierAltarCommit(
  int Cobalt,
  int Mythril,
  int Adamantite);
