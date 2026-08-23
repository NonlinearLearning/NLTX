using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerCommandCommitResult(
  bool Succeeded,
  int AppliedTileCount,
  int AppliedLiquidCount,
  string? FailureReason)
{
  public static LegacyTileRunnerCommandCommitResult Failed(string reason)
  {
    return new LegacyTileRunnerCommandCommitResult(false, 0, 0, reason);
  }
}

public static class LegacyTileRunnerCommandCommitBoundary
{
  public static bool TryCommit(
    WorldGrid world,
    LegacyTileRunnerPassCommandBatch passBatch,
    IReadOnlyCollection<LiquidDefinition> liquidDefinitions,
    out LegacyTileRunnerCommandCommitResult result)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(passBatch);
    ArgumentNullException.ThrowIfNull(liquidDefinitions);

    if (!ValidatePassCommands(
          world,
          passBatch.TileCommands,
          passBatch.LiquidCommands,
          liquidDefinitions,
          out string? failureReason))
    {
      result = LegacyTileRunnerCommandCommitResult.Failed(failureReason!);
      return false;
    }

    if (!new TileChangeCommitSystem().TryCommit(
          world,
          passBatch.TileCommands,
          out TileChangeCommitResult tileResult))
    {
      result = LegacyTileRunnerCommandCommitResult.Failed(tileResult.FailureReason!);
      return false;
    }

    if (!new LiquidChangeCommitSystem().TryCommit(
          world,
          passBatch.LiquidCommands,
          liquidDefinitions,
          out LiquidChangeCommitResult liquidResult))
    {
      result = LegacyTileRunnerCommandCommitResult.Failed(liquidResult.FailureReason!);
      return false;
    }

    result = new LegacyTileRunnerCommandCommitResult(
      true,
      tileResult.AppliedCount,
      liquidResult.AppliedCount,
      null);
    return true;
  }

  public static bool TryCommit(
    WorldGrid world,
    LegacyTileRunnerCommandBatch batch,
    IReadOnlyCollection<LiquidDefinition> liquidDefinitions,
    out LegacyTileRunnerCommandCommitResult result)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(batch);
    ArgumentNullException.ThrowIfNull(liquidDefinitions);

    TileChangeCommand? tileCommand = batch.TileCommand;
    LiquidChangeCommand? liquidCommand = batch.LiquidCommand;
    if (tileCommand.HasValue && liquidCommand.HasValue &&
        tileCommand.Value.Sequence >= liquidCommand.Value.Sequence)
    {
      result = LegacyTileRunnerCommandCommitResult.Failed(
        "TileRunner tile command must precede its liquid command.");
      return false;
    }

    if (tileCommand.HasValue && !ValidateTileCommand(world, tileCommand.Value, out string? tileFailure))
    {
      result = LegacyTileRunnerCommandCommitResult.Failed(tileFailure!);
      return false;
    }

    if (liquidCommand.HasValue &&
        !ValidateLiquidCommand(world, liquidCommand.Value, liquidDefinitions, out string? liquidFailure))
    {
      result = LegacyTileRunnerCommandCommitResult.Failed(liquidFailure!);
      return false;
    }

    if (!ValidateSectionCapacity(world, tileCommand, liquidCommand, out string? capacityFailure))
    {
      result = LegacyTileRunnerCommandCommitResult.Failed(capacityFailure!);
      return false;
    }

    if (tileCommand.HasValue)
    {
      if (!new TileChangeCommitSystem().TryCommit(
            world,
            new[] { tileCommand.Value },
            out TileChangeCommitResult tileResult))
      {
        result = LegacyTileRunnerCommandCommitResult.Failed(tileResult.FailureReason!);
        return false;
      }
    }

    if (liquidCommand.HasValue)
    {
      if (!new LiquidChangeCommitSystem().TryCommit(
            world,
            new[] { liquidCommand.Value },
            liquidDefinitions,
            out LiquidChangeCommitResult liquidResult))
      {
        result = LegacyTileRunnerCommandCommitResult.Failed(liquidResult.FailureReason!);
        return false;
      }
    }

    result = new LegacyTileRunnerCommandCommitResult(
      true,
      tileCommand.HasValue ? 1 : 0,
      liquidCommand.HasValue ? 1 : 0,
      null);
    return true;
  }

  private static bool ValidateTileCommand(
    WorldGrid world,
    TileChangeCommand command,
    out string? failureReason)
  {
    if (command.Sequence < 0 || command.Sequence == long.MaxValue)
    {
      failureReason = "TileRunner tile command sequence was invalid.";
      return false;
    }

    if (!world.Contains(command.X, command.Y))
    {
      failureReason = "TileRunner tile command was outside the world.";
      return false;
    }

    if (!Enum.IsDefined(command.Kind))
    {
      failureReason = "TileRunner tile command kind was invalid.";
      return false;
    }

    failureReason = null;
    return true;
  }

  private static bool ValidateLiquidCommand(
    WorldGrid world,
    LiquidChangeCommand command,
    IReadOnlyCollection<LiquidDefinition> liquidDefinitions,
    out string? failureReason)
  {
    if (command.Sequence < 0 || command.Sequence == long.MaxValue)
    {
      failureReason = "TileRunner liquid command sequence was invalid.";
      return false;
    }

    if (!world.Contains(command.X, command.Y))
    {
      failureReason = "TileRunner liquid command was outside the world.";
      return false;
    }

    foreach (LiquidDefinition definition in liquidDefinitions)
    {
      if (definition.Type == command.Type)
      {
        if (command.Amount <= definition.MaxAmount)
        {
          failureReason = null;
          return true;
        }

        failureReason = "TileRunner liquid command amount exceeded its definition.";
        return false;
      }
    }

    failureReason = "TileRunner liquid command type was not defined.";
    return false;
  }

  private static bool ValidateSectionCapacity(
    WorldGrid world,
    TileChangeCommand? tileCommand,
    LiquidChangeCommand? liquidCommand,
    out string? failureReason)
  {
    Dictionary<WorldSectionCoordinates, int> versionDeltas = new();
    if (tileCommand.HasValue)
    {
      TileChangeCommand command = tileCommand.Value;
      WorldTile current = world.GetTile(command.X, command.Y);
      WorldTile projected = TileMutationProjection.Apply(current, command);
      if (projected != current)
      {
        AddVersionDelta(world.GetSectionCoordinates(command.X, command.Y), versionDeltas);
      }
    }

    if (liquidCommand.HasValue)
    {
      LiquidChangeCommand command = liquidCommand.Value;
      WorldTile current = world.GetTile(command.X, command.Y);
      if (current.LiquidAmount != command.Amount || current.LiquidType != command.Type)
      {
        AddVersionDelta(world.GetSectionCoordinates(command.X, command.Y), versionDeltas);
      }
    }

    foreach (KeyValuePair<WorldSectionCoordinates, int> entry in versionDeltas)
    {
      long currentVersion = world.GetSectionVersion(entry.Key);
      if (currentVersion > long.MaxValue - entry.Value)
      {
        failureReason = "TileRunner command batch would exhaust a section version.";
        return false;
      }
    }

    failureReason = null;
    return true;
  }

  private static bool ValidatePassCommands(
    WorldGrid world,
    IReadOnlyCollection<TileChangeCommand> tileCommands,
    IReadOnlyCollection<LiquidChangeCommand> liquidCommands,
    IReadOnlyCollection<LiquidDefinition> liquidDefinitions,
    out string? failureReason)
  {
    Dictionary<WorldSectionCoordinates, int> versionDeltas = new();
    foreach (TileChangeCommand command in tileCommands)
    {
      if (!ValidateTileCommand(world, command, out failureReason))
      {
        return false;
      }

      WorldTile projected = TileMutationProjection.Apply(
        world.GetTile(command.X, command.Y),
        command);
      if (projected != world.GetTile(command.X, command.Y))
      {
        AddVersionDelta(world.GetSectionCoordinates(command.X, command.Y), versionDeltas);
      }
    }

    foreach (LiquidChangeCommand command in liquidCommands)
    {
      if (!ValidateLiquidCommand(world, command, liquidDefinitions, out failureReason))
      {
        return false;
      }

      WorldTile current = world.GetTile(command.X, command.Y);
      if (current.LiquidAmount != command.Amount || current.LiquidType != command.Type)
      {
        AddVersionDelta(world.GetSectionCoordinates(command.X, command.Y), versionDeltas);
      }
    }

    foreach (KeyValuePair<WorldSectionCoordinates, int> entry in versionDeltas)
    {
      if (world.GetSectionVersion(entry.Key) > long.MaxValue - entry.Value)
      {
        failureReason = "TileRunner pass would exhaust a section version.";
        return false;
      }
    }

    failureReason = null;
    return true;
  }

  private static void AddVersionDelta(
    WorldSectionCoordinates coordinates,
    Dictionary<WorldSectionCoordinates, int> versionDeltas)
  {
    versionDeltas[coordinates] = versionDeltas.TryGetValue(coordinates, out int current)
      ? current + 1
      : 1;
  }
}
