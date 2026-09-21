namespace Terraria.Player.Progression;

public sealed class PlayerUnlockProgressionLedgerComponent
{
  public bool UnlockedBiomeTorches { get; internal set; }

  public bool AteArtisanBread { get; internal set; }

  public bool UnlockedSuperCart { get; internal set; }

  public bool EnabledSuperCart { get; internal set; } = true;
}
