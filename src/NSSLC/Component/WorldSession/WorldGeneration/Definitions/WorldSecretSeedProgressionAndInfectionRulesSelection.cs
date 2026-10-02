namespace Terraria.WorldGeneration.Definitions;

public readonly record struct WorldSecretSeedProgressionAndInfectionRulesSelection(
  bool ErrorWorld,
  bool GraveyardBloodmoonStart,
  bool RandomSpawn,
  bool StartInHardmode,
  bool NoInfection,
  bool HallowOnTheSurface,
  bool WorldIsInfected,
  bool SurfaceIsMushrooms,
  bool SurfaceIsDesert,
  bool PooEverywhere,
  bool Vampirism,
  bool TeamBasedSpawns);
