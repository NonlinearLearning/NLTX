namespace Terraria.NonAuthoritative.Persistence;

public readonly record struct WorldTileHeaderValue(
  WorldTileHeaderCoreValue Core,
  WorldTileHeaderExtensionValue Extension);
