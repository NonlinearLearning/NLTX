namespace Terraria.Dome.Simulation.WorldGeneration.Commands;

public readonly record struct StructurePlacementCommand(
  long Sequence,
  string DefinitionId,
  int OriginX,
  int OriginY);
