namespace Terraria.WorldSession.Components;

/// <summary>
/// Carries the raw saved-tier fields that were conditionally present in a world file.
/// </summary>
public readonly record struct WorldSavedOreTierFileInput(
  int VersionNumber,
  int AltarCount,
  int? Cobalt,
  int? Mythril,
  int? Adamantite,
  int? Copper,
  int? Iron,
  int? Silver,
  int? Gold);
