namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record TownNpcSpawnCandidate(
  int Type,
  bool CanSpawn,
  bool AlreadyPresent,
  bool SpecialConditionsAllowed,
  bool HasRoom,
  bool IsTownPet,
  bool IsPrioritized);
