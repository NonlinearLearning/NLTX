namespace Terraria.Teleportation;

public readonly record struct TeleportTransitionSnapshot(
  TeleportEndpointSnapshot SourceEndpoint,
  TeleportEndpointSnapshot DestinationEndpoint,
  TeleportSubjectSnapshot Subject,
  TeleportCooldownSnapshot Cooldown);
