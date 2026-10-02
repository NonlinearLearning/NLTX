namespace Terraria.Npc;

public static class NpcSpawnLegacyWallClassificationQuery
{
  private const int LivingTreeWallType = 244;

  public static int ResolveLegacyWallType(
    in NpcSpawnLegacyWallClassificationFacts facts)
  {
    int wallType = facts.WallAboveSpawnTile;
    if (facts.WallTwoTilesAboveSpawnTile == LivingTreeWallType ||
      facts.WallOnSpawnTile == LivingTreeWallType)
    {
      wallType = LivingTreeWallType;
    }

    return wallType;
  }
}
