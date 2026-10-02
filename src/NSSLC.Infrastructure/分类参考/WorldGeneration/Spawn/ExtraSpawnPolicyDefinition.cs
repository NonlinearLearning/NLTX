namespace Terraria.WorldGeneration.Spawn;

public sealed class ExtraSpawnPolicyDefinition
{
  public ExtraSpawnPolicyDefinition(
    ExtraSpawnType spawnType = ExtraSpawnType.None,
    bool surface = false,
    bool remix = false,
    bool roundLandmass = false,
    bool skyblock = false,
    bool extraLiquid = false)
  {
    SpawnType = spawnType;
    Surface = surface;
    Remix = remix;
    RoundLandmass = roundLandmass;
    Skyblock = skyblock;
    ExtraLiquid = extraLiquid;
  }

  public ExtraSpawnType SpawnType { get; }

  public bool Surface { get; }

  public bool Remix { get; }

  public bool RoundLandmass { get; }

  public bool Skyblock { get; }

  public bool ExtraLiquid { get; }
}
