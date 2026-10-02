using Terraria.WorldSession.Session;

namespace Terraria.WorldSession.Queries;

public static class WorldSessionRuleQuery
{
  public static int GameMode(WorldSessionRuleSnapshot snapshot)
  {
    return snapshot.GameMode;
  }

  public static bool IsJourneyMode(WorldSessionRuleSnapshot snapshot)
  {
    return GameMode(snapshot) == 3;
  }

  public static bool SurviveHardcoreDeath(WorldSessionRuleSnapshot snapshot)
  {
    return snapshot.DontStarveWorld &&
      snapshot.TenthAnniversaryWorld &&
      !snapshot.GoodWorld;
  }

  public static bool OnlyShimmerOceanWorlds(WorldSessionRuleSnapshot snapshot)
  {
    return snapshot.DrunkWorld &&
      snapshot.TenthAnniversaryWorld &&
      !snapshot.RemixWorld &&
      !snapshot.ZenithWorld &&
      !snapshot.NotTheBeesWorld;
  }
}
