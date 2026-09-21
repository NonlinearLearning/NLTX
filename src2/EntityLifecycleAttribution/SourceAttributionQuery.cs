namespace Terraria.EntityLifecycleAttribution;

public static class SourceAttributionQuery
{
  public static bool TryGetPrimaryEntity(
    EntitySpawnSourceContext context,
    out EntityReference reference)
  {
    if (context.PrimaryEntity is EntityReference value && value.IsValid)
    {
      reference = value;
      return true;
    }

    reference = default;
    return false;
  }

  public static bool TryGetSecondaryEntity(
    EntitySpawnSourceContext context,
    out EntityReference reference)
  {
    if (context.SecondaryEntity is EntityReference value && value.IsValid)
    {
      reference = value;
      return true;
    }

    reference = default;
    return false;
  }
}
