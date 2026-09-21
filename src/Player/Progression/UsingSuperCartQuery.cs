namespace Terraria.Player.Progression;

public static class UsingSuperCartQuery
{
  public static bool IsEnabled(PlayerUnlockProgressionLedgerComponent ledger)
  {
    return ledger.UnlockedSuperCart && ledger.EnabledSuperCart;
  }
}
