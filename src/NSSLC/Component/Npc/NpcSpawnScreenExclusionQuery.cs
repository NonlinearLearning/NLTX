using System;

namespace Terraria.Npc;

public static class NpcSpawnScreenExclusionQuery
{
  public static bool IsSpawnTileOutsideScreen(
    int spawnTileX,
    int spawnTileY,
    in NpcSpawnScreenExclusionInputs inputs)
  {
    ArgumentNullException.ThrowIfNull(inputs.Players);
    ArgumentOutOfRangeException.ThrowIfNegative(inputs.ScreenWidthPixels);
    ArgumentOutOfRangeException.ThrowIfNegative(inputs.ScreenHeightPixels);
    ArgumentOutOfRangeException.ThrowIfNegative(inputs.SafeRangeX);
    ArgumentOutOfRangeException.ThrowIfNegative(inputs.SafeRangeY);

    int halfScreenWidth = inputs.ScreenWidthPixels / 2;
    int halfScreenHeight = inputs.ScreenHeightPixels / 2;
    int spawnLeft = spawnTileX * 16;
    int spawnTop = spawnTileY * 16;
    int spawnRight = spawnLeft + 16;
    int spawnBottom = spawnTop + 16;

    foreach (NpcSpawnScreenPlayerSnapshot player in inputs.Players)
    {
      if (!player.IsActive ||
        (inputs.DualDungeonsSeed && player.InsideUnbreakableWalls))
      {
        continue;
      }

      int screenLeft = (int)(player.CenterXInPixels -
        (float)halfScreenWidth - (float)inputs.SafeRangeX);
      int screenTop = (int)(player.CenterYInPixels -
        (float)halfScreenHeight - (float)inputs.SafeRangeY);
      int screenRight = screenLeft + inputs.ScreenWidthPixels + inputs.SafeRangeX * 2;
      int screenBottom = screenTop + inputs.ScreenHeightPixels + inputs.SafeRangeY * 2;

      if (spawnLeft < screenRight &&
        spawnRight > screenLeft &&
        spawnTop < screenBottom &&
        spawnBottom > screenTop)
      {
        return false;
      }
    }

    return true;
  }
}
