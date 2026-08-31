using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class OrePatchPlacementSystem
{
  public bool TryPrepare(
    WorldGridSnapshot snapshot,
    int originX,
    int originY,
    int worldSurfaceY,
    IReadOnlyDictionary<ushort, OrePatchTileDefinition> tileDefinitions,
    OreDefinition definition,
    ushort copperTileType,
    ushort ironTileType,
    LegacyPassRandomState random,
    TileProtectionComponent protection,
    out OrePatchPlacementPreparation preparation)
  {
    ArgumentNullException.ThrowIfNull(random);
    ushort selectedTileType = LegacyOrePatchTypePolicy.SelectTileType(
      copperTileType, ironTileType, random);
    OreDefinition selectedDefinition = new(
      definition.Id,
      selectedTileType,
      definition.MinDepth,
      definition.MaxDepth,
      definition.VeinRadius,
      definition.Priority);
    return TryPrepare(
      snapshot,
      originX,
      originY,
      worldSurfaceY,
      tileDefinitions,
      selectedDefinition,
      protection,
      out preparation);
  }

  public bool TryPrepare(
    WorldGridSnapshot snapshot,
    int originX,
    int originY,
    int worldSurfaceY,
    IReadOnlyDictionary<ushort, OrePatchTileDefinition> tileDefinitions,
    OreDefinition definition,
    TileProtectionComponent protection,
    out OrePatchPlacementPreparation preparation)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);

    OrePatchEligibilityResult eligibility = OrePatchEligibilityQuery.Evaluate(
      snapshot,
      originX,
      originY,
      worldSurfaceY,
      tileDefinitions);
    if (!eligibility.IsEligible)
    {
      preparation = default;
      return false;
    }

    OrePlacementPreparationResult transaction;
    OrePlacementTransactionSystem transactionSystem = new();
    if (!transactionSystem.TryPrepare(
          snapshot,
          definition,
          originX,
          eligibility.GroundY + 1,
          protection,
          out transaction))
    {
      preparation = default;
      return false;
    }

    preparation = new OrePatchPlacementPreparation(eligibility, transaction, originX);
    return true;
  }

  public bool TryAppendCommands(
    WorldGridSnapshot snapshot,
    OrePatchPlacementPreparation preparation,
    TileProtectionComponent protection,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commands);
    if (!preparation.IsPrepared)
    {
      return false;
    }

    return new OrePlacementTransactionSystem().TryAppendCommands(
      snapshot,
      preparation.Transaction,
      protection,
      ref state,
      commands);
  }

  public bool TryAppendLegacyTrailAndBlobCommands(
    WorldGridSnapshot snapshot,
    OrePatchPlacementPreparation preparation,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (!preparation.IsPrepared || preparation.OriginX < 0)
    {
      return false;
    }

    WorldGenerationStateComponent nextState = state;
    List<TileChangeCommand> nextCommands = new();
    if (!LegacyOrePatchTrail.TryAppendCommands(
          snapshot,
          preparation.OriginX,
          preparation.GroundY,
          preparation.Transaction.TileType,
          random,
          ref nextState,
          nextCommands,
          out LegacyOrePatchTrailEnd trailEnd) ||
        !LegacyOrePatchBlob.TryAppendCommands(
          snapshot,
          trailEnd.X,
          trailEnd.Y,
          preparation.Transaction.TileType,
          random,
          ref nextState,
          nextCommands))
    {
      return false;
    }

    commands.AddRange(nextCommands);
    state = nextState;
    return true;
  }
}
