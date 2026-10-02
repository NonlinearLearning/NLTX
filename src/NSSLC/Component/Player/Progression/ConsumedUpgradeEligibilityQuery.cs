using Terraria.Player;

namespace Terraria.Player.Progression;

public static class ConsumedUpgradeEligibilityQuery
{
  // These legacy item type values match the target and full reference snapshots.
  private const int AegisCrystalItemType = 5337;
  private const int AegisFruitItemType = 5338;
  private const int ArcaneCrystalItemType = 5339;
  private const int GalaxyPearlItemType = 5340;
  private const int GummyWormItemType = 5341;
  private const int AmbrosiaItemType = 5342;

  public static bool TryResolveEligibleUpgrade(
    in PlayerConsumedUpgradeItemUseInput input,
    PlayerConsumedProgressionLedgerComponent ledger,
    out PlayerConsumedProgressionUpgrade upgrade)
  {
    ArgumentNullException.ThrowIfNull(ledger);
    upgrade = default;
    if (input.ItemAnimation <= 0 || !input.ItemTimeIsZero)
    {
      return false;
    }

    PlayerConsumedProgressionUpgrade? resolvedUpgradeCandidate = input.ItemType switch
    {
      AegisCrystalItemType => PlayerConsumedProgressionUpgrade.AegisCrystal,
      AegisFruitItemType => PlayerConsumedProgressionUpgrade.AegisFruit,
      ArcaneCrystalItemType => PlayerConsumedProgressionUpgrade.ArcaneCrystal,
      GalaxyPearlItemType => PlayerConsumedProgressionUpgrade.GalaxyPearl,
      GummyWormItemType => PlayerConsumedProgressionUpgrade.GummyWorm,
      AmbrosiaItemType => PlayerConsumedProgressionUpgrade.Ambrosia,
      _ => null,
    };

    if (resolvedUpgradeCandidate is not { } resolvedUpgrade ||
      !CanConsume(ledger, resolvedUpgrade))
    {
      return false;
    }

    upgrade = resolvedUpgrade;
    return true;
  }

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
