using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyWallFrameCommand(
  long Sequence,
  int X,
  int Y,
  WorldTile Result,
  int NeighborMask,
  int FrameLookupIndex,
  bool ResetFrame,
  string Source,
  int SourceLine,
  int Priority = 0,
  long? ExpectedSectionVersion = null);
