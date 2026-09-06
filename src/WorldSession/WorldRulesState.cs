namespace Terraria.WorldSession.Components;

public sealed class WorldRulesState
{
  public WorldGameMode GameMode;
  public bool HardMode;
  public WorldSecretSeedFlags SecretSeeds;
  public WorldEvilType WorldEvil;
  public OreTierState SavedOreTiers = OreTierState.Uninitialized;

  public bool IsJourneyMode => GameMode == WorldGameMode.Journey;
  public bool UsesDualDungeons =>
    (SecretSeeds & WorldSecretSeedFlags.DualDungeons) != 0;
  public bool IsSkyblockWorld =>
    (SecretSeeds & WorldSecretSeedFlags.Skyblock) != 0;
}
