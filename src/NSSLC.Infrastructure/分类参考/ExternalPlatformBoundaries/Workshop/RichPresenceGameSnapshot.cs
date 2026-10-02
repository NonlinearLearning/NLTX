namespace Terraria.ExternalPlatformBoundaries.Workshop;

public readonly record struct RichPresenceGameSnapshot(
  bool IsMenu,
  bool IsServer,
  bool IsMultiplayer);
