using Terraria.WorldSession.Session;

namespace Terraria.WorldSession.Queries;

public static class WorldSessionQuery
{
  public static string? SavePath(StoragePathValue? path)
  {
    return path?.Value;
  }

  public static int PersistentWorldId(ActiveWorldMetadataProjection? metadata)
  {
    return metadata?.PersistentWorldId ?? 0;
  }

  public static string? PlayerPath(StoragePathValue? path)
  {
    return path?.Value;
  }

  public static string? WorldPath(ActiveWorldMetadataProjection? metadata)
  {
    return metadata?.Path;
  }
}
