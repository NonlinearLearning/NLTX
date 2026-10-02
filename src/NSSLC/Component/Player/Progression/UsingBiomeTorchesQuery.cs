namespace Terraria.Player.Progression;

public static class UsingBiomeTorchesQuery
{
  public static bool IsEnabled(
    PlayerUnlockProgressionLedgerComponent ledger,
    bool biomeTorchPreferenceEnabled)
  {
    return ledger.UnlockedBiomeTorches && biomeTorchPreferenceEnabled;
  }
}
