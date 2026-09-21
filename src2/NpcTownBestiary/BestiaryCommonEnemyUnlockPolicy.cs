namespace Terraria.NpcTownBestiary;

public readonly record struct BestiaryCommonEnemyUnlockPolicy
{
  public BestiaryCommonEnemyUnlockPolicy(bool quickUnlock, int fullKillCountNeeded)
  {
    if (fullKillCountNeeded <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(fullKillCountNeeded));
    }

    QuickUnlock = quickUnlock;
    FullKillCountNeeded = fullKillCountNeeded;
  }

  public bool QuickUnlock { get; }

  public int FullKillCountNeeded { get; }
}
