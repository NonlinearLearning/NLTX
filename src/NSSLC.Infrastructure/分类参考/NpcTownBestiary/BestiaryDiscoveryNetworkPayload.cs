namespace Terraria.NpcTownBestiary;

// crossSubsystemOwner: integration-review
public readonly record struct BestiaryDiscoveryNetworkPayload(
  BestiaryDiscoveryEventKind Kind,
  NpcNetId NpcNetId,
  int KillCount);
