namespace Terraria.WorldStorage;

public readonly record struct PylonRegistryEntry(
  TileCoordinate Position,
  byte Kind,
  TileEntityId TileEntityId,
  bool IsValid)
{
  public bool CanTeleport => IsValid && Kind != 0;
}
