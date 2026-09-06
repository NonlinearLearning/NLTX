namespace Terraria.WorldGeneration.Components;

public sealed class WorldRulesState
{
  public WorldGameMode GameMode;
  public bool HardMode;
  public WorldSecretSeedFlags SecretSeeds;
  public WorldEvilType WorldEvil;
  public OreTierState OreTiers;

  public bool IsJourneyMode => GameMode == WorldGameMode.Journey;
  public bool IsExpertMode => GameMode == WorldGameMode.Expert;
  public bool IsMasterMode => GameMode == WorldGameMode.Master;
  public bool UsesDualDungeons =>
    (SecretSeeds & WorldSecretSeedFlags.DualDungeons) != 0;
  public bool IsSkyblockWorld =>
    (SecretSeeds & WorldSecretSeedFlags.Skyblock) != 0;
}
