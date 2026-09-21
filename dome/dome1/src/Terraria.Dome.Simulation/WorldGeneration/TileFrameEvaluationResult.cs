using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record TileFrameEvaluationResult(
  bool IsSupported,
  short FrameX,
  short FrameY,
  bool? IsHalfBrick,
  byte? Slope,
  bool ShouldKill,
  IReadOnlyList<TileFrameCoordinate> AffectedCoordinates,
  TileFrameClassificationKind Classification = TileFrameClassificationKind.InactiveOrUnsupported);
