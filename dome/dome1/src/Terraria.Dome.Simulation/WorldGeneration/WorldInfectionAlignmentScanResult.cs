namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WorldInfectionAlignmentScanResult(
  WorldInfectionAlignmentScanState State,
  bool Scanned,
  int ColumnX,
  WorldInfectionAlignmentSnapshot Column,
  bool HasPublished,
  WorldInfectionAlignmentSnapshot Published);
