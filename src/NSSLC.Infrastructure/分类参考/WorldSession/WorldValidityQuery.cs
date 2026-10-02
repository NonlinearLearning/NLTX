namespace Terraria.NonAuthoritative.WorldSession;

public static class WorldValidityQuery
{
  public static bool IsPlayable(WorldValiditySnapshot validity)
  {
    ArgumentNullException.ThrowIfNull(validity);
    return validity.IsValid;
  }

  public static string GetMapFileName(WorldDescriptorPersistenceSnapshot identity)
  {
    ArgumentNullException.ThrowIfNull(identity);
    return identity.MapFileName;
  }

  public static string GetWorldSizeName(WorldDescriptorPersistenceSnapshot identity)
  {
    ArgumentNullException.ThrowIfNull(identity);
    return identity.WorldSizeName;
  }
}
