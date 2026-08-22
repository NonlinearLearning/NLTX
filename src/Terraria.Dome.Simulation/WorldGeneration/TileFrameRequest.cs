using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record TileFrameRequest(
  int X,
  int Y,
  TileFrameMutationKind MutationKind,
  WorldGridSnapshot Snapshot,
  IReadOnlyCollection<TileChangeCommand> PendingMutations);
