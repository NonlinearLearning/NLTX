using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyWebsCandidateSelector
{
  public static bool TrySelect(
    WorldGridSnapshot snapshot,
    LegacyPassRandomState random,
    int initialX,
    int initialY,
    int worldSurfaceY,
    int worldSurfaceLowY,
    out LegacyWebsCandidate candidate)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    candidate = default;
    if (initialX < 0 || initialX >= snapshot.Metadata.Width || initialY < 0 ||
        initialY >= snapshot.Metadata.Height || worldSurfaceY < 0 ||
        worldSurfaceLowY < 0 || worldSurfaceLowY > worldSurfaceY)
    {
      throw new ArgumentOutOfRangeException(nameof(initialX));
    }

    WorldTile initialTile = snapshot.GetTile(initialX, initialY);
    if (initialTile.IsActive || (initialY <= worldSurfaceY && initialTile.WallType == 0))
    {
      return false;
    }

    int y = initialY;
    while (!snapshot.GetTile(initialX, y).IsActive && y > worldSurfaceLowY)
    {
      y--;
    }

    y++;
    int direction = random.Next(2) == 0 ? -1 : 1;
    int x = initialX;
    while (!snapshot.GetTile(x, y).IsActive && x > 10 && x < snapshot.Metadata.Width - 10)
    {
      x += direction;
    }

    x -= direction;
    WorldTile recoveredTile = snapshot.GetTile(x, y);
    if (y <= worldSurfaceY && recoveredTile.WallType == 0)
    {
      return false;
    }

    candidate = new LegacyWebsCandidate(x, y, direction);
    return true;
  }
}
