namespace Terraria.WorldStorage;

public readonly record struct PylonProjectionMessage(
  uint Revision,
  PylonProjectionMessageKind Kind,
  PylonRegistryEntry Entry);
