using Terraria.WorldSession.Session;

namespace Terraria.WorldSession.Queries;

public static class PersistentWorldIdQuery
{
  public static int Read(ActiveWorldMetadataProjection? metadata)
  {
    return metadata?.PersistentWorldId ?? 0;
  }
}
