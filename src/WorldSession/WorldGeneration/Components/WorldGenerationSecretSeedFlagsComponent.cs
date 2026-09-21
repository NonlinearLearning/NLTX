namespace Terraria.WorldGeneration.Components;

public sealed class WorldGenerationSecretSeedFlagsComponent
{
  public WorldGenerationSecretSeedFlagsComponent(
    bool remixWorldGeneration = false,
    bool everythingWorldGeneration = false,
    bool noTrapsWorldGeneration = false,
    bool drunkWorldGeneration = false,
    bool getGoodWorldGeneration = false,
    bool tenthAnniversaryWorldGeneration = false,
    bool dontStarveWorldGeneration = false,
    bool notTheBeesWorld = false,
    bool skyblockWorldGeneration = false)
  {
    RemixWorldGeneration = remixWorldGeneration;
    EverythingWorldGeneration = everythingWorldGeneration;
    NoTrapsWorldGeneration = noTrapsWorldGeneration;
    DrunkWorldGeneration = drunkWorldGeneration;
    GetGoodWorldGeneration = getGoodWorldGeneration;
    TenthAnniversaryWorldGeneration = tenthAnniversaryWorldGeneration;
    DontStarveWorldGeneration = dontStarveWorldGeneration;
    NotTheBeesWorld = notTheBeesWorld;
    SkyblockWorldGeneration = skyblockWorldGeneration;
  }

  public bool RemixWorldGeneration { get; }

  public bool EverythingWorldGeneration { get; }

  public bool NoTrapsWorldGeneration { get; }

  public bool DrunkWorldGeneration { get; }

  public bool GetGoodWorldGeneration { get; }

  public bool TenthAnniversaryWorldGeneration { get; }

  public bool DontStarveWorldGeneration { get; }

  public bool NotTheBeesWorld { get; }

  public bool SkyblockWorldGeneration { get; }

  public bool DrunkWorldGenerationText => DrunkWorldGeneration;
}
