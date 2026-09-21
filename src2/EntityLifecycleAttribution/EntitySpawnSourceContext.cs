namespace Terraria.EntityLifecycleAttribution;

public readonly record struct EntitySpawnSourceContext(
  EntitySpawnSourceKind Kind,
  EntityReference? PrimaryEntity,
  EntityReference? SecondaryEntity,
  TileCoordinate? TileCoords,
  int? SourceId,
  int? AmmoItemIdUsed,
  int? MountId)
{
  public bool IsKnown => Kind != EntitySpawnSourceKind.Unknown;
}
