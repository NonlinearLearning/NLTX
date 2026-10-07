namespace Terraria.Npc;

public static class NpcHomeReturnDestinationQuery
{
  public static bool TryFindDestination(
    int homeTileX,
    int homeTileY,
    int width,
    int height,
    INpcHomeReturnCollisionQuery collisionQuery,
    out NpcHomeReturnDestination destination)
  {
    ArgumentNullException.ThrowIfNull(collisionQuery);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

    for (int candidateIndex = 0; candidateIndex < 3; candidateIndex++)
    {
      int candidateOffset = candidateIndex switch
      {
        1 => -1,
        2 => 1,
        _ => 0,
      };
      int candidateTileX = homeTileX + candidateOffset;
      if (HasSolidHomeArea(collisionQuery, candidateTileX, homeTileY))
      {
        continue;
      }

      destination = new NpcHomeReturnDestination(
        new System.Numerics.Vector2(
          candidateTileX * 16f + 8f - width / 2,
          homeTileY * 16f - height - 0.1f),
        candidateOffset);
      return true;
    }

    destination = default;
    return false;
  }

  private static bool HasSolidHomeArea(
    INpcHomeReturnCollisionQuery collisionQuery,
    int candidateTileX,
    int homeTileY)
  {
    for (int tileX = candidateTileX - 1; tileX <= candidateTileX + 1; tileX++)
    {
      for (int tileY = homeTileY - 3; tileY <= homeTileY - 1; tileY++)
      {
        if (collisionQuery.IsSolidTile(tileX, tileY))
        {
          return true;
        }
      }
    }

    return false;
  }
}
