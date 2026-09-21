namespace Terraria.NpcTownBestiary;

public sealed class BestiaryStatsRefreshAdapter
{
  public BestiaryNpcStatsView Refresh(NpcReadView npc, NpcNetId npcNetId)
  {
    return new BestiaryNpcStatsView(
      npcNetId,
      npc.Damage,
      npc.LifeMax,
      npc.MonetaryValue,
      npc.Defense,
      npc.KnockbackResist,
      false);
  }
}
