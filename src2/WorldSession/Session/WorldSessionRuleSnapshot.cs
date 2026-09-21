namespace Terraria.WorldSession.Session;

public readonly record struct WorldSessionRuleSnapshot(
  int GameModeValue,
  bool GoodWorld,
  bool DontStarveWorld,
  bool TenthAnniversaryWorld,
  bool DrunkWorld,
  bool RemixWorld,
  bool ZenithWorld,
  bool NotTheBeesWorld)
{
  public int GameMode => GameModeValue;
}
