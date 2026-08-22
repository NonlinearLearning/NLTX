namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TreeLeafScanResult(
  bool FoundTopTile,
  int TreeHeight,
  int TreeFrame,
  int PassStyle);
