using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class OrdinaryTreePlacementSystem
{
  private const int MaximumHeight = 16;
  private const int MinimumHeight = 5;

  public bool TryPrepareWithHeightSelection(
    WorldGridSnapshot snapshot,
    int originX,
    int groundY,
    int treeHeightAddon,
    IReadOnlyDictionary<ushort, TreeWallDefinition> wallDefinitions,
    bool ignoreWalls,
    ref GenerationRandomState random,
    out OrdinaryTreePlacementPreparation preparation,
    out string? failureReason)
  {
    OrdinaryTreeHeightResult height = OrdinaryTreeHeightPolicy.Next(random, treeHeightAddon);
    bool prepared = TryPrepare(
      snapshot,
      originX,
      groundY,
      height.Height,
      wallDefinitions,
      ignoreWalls,
      out preparation,
      out failureReason);
    if (prepared)
    {
      random = height.State;
    }

    return prepared;
  }

  public bool TryPrepare(
    WorldGridSnapshot snapshot,
    int originX,
    int groundY,
    int height,
    IReadOnlyDictionary<ushort, TreeWallDefinition> wallDefinitions,
    bool ignoreWalls,
    out OrdinaryTreePlacementPreparation preparation,
    out string? failureReason)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(wallDefinitions);
    preparation = default;
    failureReason = null;
    if (height < MinimumHeight || height > MaximumHeight)
    {
      failureReason = "ordinary tree height is outside the bounded range";
      return false;
    }

    OrdinaryTreeGrowthEligibilityResult eligibility =
      OrdinaryTreeGrowthEligibilityQuery.Evaluate(
        snapshot,
        originX,
        groundY,
        wallDefinitions,
        ignoreWalls);
    if (!eligibility.IsEligible)
    {
      failureReason = eligibility.Reason.ToString();
      return false;
    }

    if (!TreeCanopyClearanceQuery.IsClear(
          snapshot,
          originX - 2,
          originX + 2,
          eligibility.GroundY - height - 4,
          eligibility.GroundY - 1,
          20,
          CommonSaplingTileRegistry.RegisterDefaults()))
    {
      failureReason = "ordinary tree canopy is blocked";
      return false;
    }

    preparation = new OrdinaryTreePlacementPreparation(originX, eligibility.GroundY, height);
    return true;
  }

  public bool TryAppendCommands(
    WorldGridSnapshot snapshot,
    OrdinaryTreePlacementPreparation preparation,
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

    return new OrdinaryTreeTrunkCommandSystem().TryAppendCommands(
      snapshot,
      preparation.OriginX,
      preparation.GroundY,
      preparation.Height,
      protection,
      ref state,
      commands);
  }
}
