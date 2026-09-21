using Terraria.Player;

namespace Terraria.Player.Progression;

public static class ConsumedUpgradeEligibilityQuery
{
  public static bool CanConsume(
    PlayerConsumedProgressionLedgerComponent ledger,
    PlayerConsumedProgressionUpgrade upgrade)
  {
    return IsSupported(upgrade) && !IsConsumed(ledger, upgrade);
  }

  public static bool IsConsumed(
    PlayerConsumedProgressionLedgerComponent ledger,
    PlayerConsumedProgressionUpgrade upgrade)
  {
    return upgrade switch
    {
      PlayerConsumedProgressionUpgrade.AegisCrystal => ledger.UsedAegisCrystal,
      PlayerConsumedProgressionUpgrade.AegisFruit => ledger.UsedAegisFruit,
      PlayerConsumedProgressionUpgrade.ArcaneCrystal => ledger.UsedArcaneCrystal,
      PlayerConsumedProgressionUpgrade.GalaxyPearl => ledger.UsedGalaxyPearl,
      PlayerConsumedProgressionUpgrade.GummyWorm => ledger.UsedGummyWorm,
      PlayerConsumedProgressionUpgrade.Ambrosia => ledger.UsedAmbrosia,
      _ => false,
    };
  }

  public static bool IsSupported(PlayerConsumedProgressionUpgrade upgrade)
  {
    return upgrade is
      PlayerConsumedProgressionUpgrade.AegisCrystal or
      PlayerConsumedProgressionUpgrade.AegisFruit or
      PlayerConsumedProgressionUpgrade.ArcaneCrystal or
      PlayerConsumedProgressionUpgrade.GalaxyPearl or
      PlayerConsumedProgressionUpgrade.GummyWorm or
      PlayerConsumedProgressionUpgrade.Ambrosia;
  }
}
