namespace Terraria.WorldGeneration.Dungeon.Bounds;

public sealed class DungeonWallProgressionDefinition
{
  public DungeonWallProgressionDefinition(
    int earlyGame,
    int evilBoss,
    int jungleBoss,
    int dungeon,
    int hallow,
    int temple)
  {
    int[] tiers = [earlyGame, evilBoss, jungleBoss, dungeon, hallow, temple];
    if (tiers.Any(tier => tier < 0))
    {
      throw new ArgumentOutOfRangeException(nameof(earlyGame));
    }

    EarlyGame = earlyGame;
    EvilBoss = evilBoss;
    JungleBoss = jungleBoss;
    Dungeon = dungeon;
    Hallow = hallow;
    Temple = temple;
  }

  public int EarlyGame { get; }

  public int EvilBoss { get; }

  public int JungleBoss { get; }

  public int Dungeon { get; }

  public int Hallow { get; }

  public int Temple { get; }
}
