namespace Terraria.NpcTownBestiary;

public static class BestiaryInfoElementProjection
{
  public static int GetKillCount(
    BestiaryKillCountStateComponent kills,
    BestiaryCreditId creditId)
  {
    ArgumentNullException.ThrowIfNull(kills);
    return kills.GetCount(creditId);
  }

  public static BestiaryInfoElementState CreateStatsElement(
    BestiaryStatsRefreshAdapter adapter,
    NpcReadView npc,
    NpcNetId npcNetId,
    bool hideStats)
  {
    ArgumentNullException.ThrowIfNull(adapter);
    BestiaryNpcStatsView stats = adapter.Refresh(npc, npcNetId) with { HideStats = hideStats };
    return new BestiaryInfoElementState(stats: stats);
  }
}
