namespace Terraria.EntityLifecycleAttribution;

public static class LegacyEntitySourceAdapter
{
  public static bool TryAdapt(
    LegacyEntitySourceRecord source,
    out EntitySpawnSourceContext context)
  {
    if (source.Kind == EntitySpawnSourceKind.Unknown)
    {
      context = default;
      return false;
    }

    if (source.PrimaryEntity is { IsValid: false } ||
        source.SecondaryEntity is { IsValid: false } ||
        source.TileCoords is { IsValid: false } ||
        source.SourceId is < 0 ||
        source.AmmoItemIdUsed is < 0 ||
        source.MountId is < 0)
    {
      context = default;
      return false;
    }

    context = new EntitySpawnSourceContext(
      source.Kind,
      source.PrimaryEntity,
      source.SecondaryEntity,
      source.TileCoords,
      source.SourceId,
      source.AmmoItemIdUsed,
      source.MountId);
    return true;
  }
}
