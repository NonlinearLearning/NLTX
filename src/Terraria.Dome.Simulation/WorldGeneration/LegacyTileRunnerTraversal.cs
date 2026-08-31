using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyTileRunnerTraversal
{
  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyTileRunnerPassInvocation invocation,
    LegacyPassRandomState random,
    int worldSurfaceY,
    int rockLayerY,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    LegacyTileRunnerLiquidContext? liquidContext = null,
    List<LiquidChangeCommand>? liquidCommands = null,
    IDictionary<(int X, int Y), WorldTile>? projectedTiles = null)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(invocation);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if ((liquidContext is null) != (liquidCommands is null))
    {
      throw new ArgumentException(
        "TileRunner liquid context and output must be supplied together.");
    }

    liquidContext?.Validate();

    LegacyTileRunnerRequest request = invocation.Request;
    projectedTiles ??= new Dictionary<(int X, int Y), WorldTile>();
    LegacyTileRunnerInitialization initialization = LegacyTileRunnerInitializationPolicy.Create(
      request.SpeedX,
      request.SpeedY,
      random.Next(-10, 11),
      random.Next(-10, 11),
      liquidTypeRoll3: 0,
      random.Next(4),
      notTheBees: false,
      dontStarve: false,
      remixWorld: false,
      drunkWorld: false,
      tenthAnniversary: false,
      getGoodWorld: false);
    double centerX = request.X;
    double centerY = request.Y;
    double directionX = initialization.DirectionX;
    double directionY = initialization.DirectionY;
    double remainingSteps = request.Steps;
    while (remainingSteps > 0)
    {
      LegacyTileRunnerEnvelope envelope = LegacyTileRunnerEnvelopePolicy.Advance(
        centerX,
        centerY,
        request.Strength,
        request.Steps,
        remainingSteps,
        directionX,
        directionY,
        snapshot.Metadata.Width,
        snapshot.Metadata.Height);
      AppendEnvelopeCommands(
        snapshot,
        invocation,
        random,
        centerX,
        centerY,
        worldSurfaceY,
        envelope,
        projectedTiles,
        ref state,
        commands,
        liquidContext,
        liquidCommands);
      LegacyTileRunnerDirection direction = LegacyTileRunnerDirectionPolicy.Finalize(
        directionX,
        directionY,
        request.TileType,
        drunkWorld: false,
        request.NoYChange,
        envelope.Strength,
        centerY,
        rockLayerY,
        snapshot.Metadata.Height,
        random.Next(-10, 11),
        drunkXRoll: 0,
        yRoll: random.Next(-10, 11));
      centerX = envelope.NextCenterX;
      centerY = envelope.NextCenterY;
      directionX = direction.X;
      directionY = direction.Y;
      remainingSteps = envelope.RemainingSteps;
    }
  }

  private static void AppendEnvelopeCommands(
    WorldGridSnapshot snapshot,
    LegacyTileRunnerPassInvocation invocation,
    LegacyPassRandomState random,
    double centerX,
    double centerY,
    int worldSurfaceY,
    LegacyTileRunnerEnvelope envelope,
    IDictionary<(int X, int Y), WorldTile> pendingTiles,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    LegacyTileRunnerLiquidContext? liquidContext,
    List<LiquidChangeCommand>? liquidCommands)
  {
    for (int x = envelope.MinX; x < envelope.MaxXExclusive; x++)
    {
      for (int y = envelope.MinY; y < envelope.MaxYExclusive; y++)
      {
        (int X, int Y) coordinates = (x, y);
        WorldTile tile = pendingTiles.TryGetValue(coordinates, out WorldTile pending)
          ? pending
          : snapshot.GetTile(x, y);
        LegacyTileRunnerTileState tileState = new(
          tile.IsActive,
          tile.Type,
          LegacyTileRunnerCandidateRegistry.IsFrameImportant(tile.Type),
          LegacyTileRunnerCandidateRegistry.IsTileCut(tile.Type));
        if (!LegacyTileRunnerCandidateQuery.ShouldMutate(
              tileState,
              x,
              y,
              centerX,
              centerY,
              invocation.Request.Strength,
              random.Next(-10, 11),
              invocation.Request.IgnoreTileType))
        {
          continue;
        }

        if (invocation.Request.TileType < 0 && tile.IsActive && tile.Type == 53)
        {
          continue;
        }

        bool injectsLiquid = liquidContext is not null && liquidCommands is not null &&
          liquidContext.ShouldInject(invocation.Request.TileType, tile.IsActive, y);
        int surfaceRandomOffset = invocation.Request.TileType == 59 &&
          invocation.Request.Overwrite && tile.IsActive && tile.Type == 1
          ? random.Next(-50, 50)
          : 0;
        TileChangeCommand? tileCommand = CreateTileCommand(
          invocation,
          tile,
          x,
          y,
          surfaceRandomOffset,
          worldSurfaceY,
          state.ReserveSequence());
        if (tileCommand.HasValue)
        {
          WorldTile projectedTile = TileMutationProjection.Apply(tile, tileCommand.Value);
          if (projectedTile != tile)
          {
            commands.Add(tileCommand.Value);
          }

          tile = projectedTile;
          pendingTiles[coordinates] = tile;
        }

        if (injectsLiquid)
        {
          LiquidChangeCommand liquidCommand = new(
            state.ReserveSequence(),
            x,
            y,
            byte.MaxValue,
            liquidContext!.ResolveInjectedLiquidType(y),
            Source: CreateSource(invocation));
          liquidCommands!.Add(liquidCommand);
          tile = tile with
          {
            LiquidAmount = liquidCommand.Amount,
            LiquidType = liquidCommand.Type
          };
          pendingTiles[coordinates] = tile;
        }

        if (ShouldWriteDirtWall(invocation.Request, y, worldSurfaceY))
        {
          TileChangeCommand wallCommand = new(
            state.ReserveSequence(),
            x,
            y,
            TileChangeKind.SetWall,
            0,
            WallType: 2,
            Source: CreateSource(invocation));
          commands.Add(wallCommand);
          pendingTiles[coordinates] = TileMutationProjection.Apply(tile, wallCommand);
        }
      }
    }
  }

  private static TileChangeCommand? CreateTileCommand(
    LegacyTileRunnerPassInvocation invocation,
    WorldTile current,
    int x,
    int y,
    int surfaceRandomOffset,
    int worldSurfaceY,
    long sequence)
  {
    LegacyTileRunnerRequest request = invocation.Request;
    string source = CreateSource(invocation);
    if (request.TileType < 0)
    {
      if (!current.IsActive)
      {
        return null;
      }

      return new TileChangeCommand(
        sequence,
        x,
        y,
        TileChangeKind.Kill,
        0,
        PreserveLiquid: request.TileType == -2,
        PreserveTileState: true,
        Source: source);
    }

    LegacyTileRunnerOverrideContext context = new(
      current.Type,
      request.TileType,
      current.IsActive,
      TargetIsStone: LegacyTileRunnerTargetRegistry.IsStone(request.TileType),
      TargetIsOre: LegacyTileRunnerTargetRegistry.IsOre(request.TileType),
      LegacyGenerationClearabilityPolicy.CanClear((int)current.Type, false),
      IsInUndergroundDesert: false,
      y,
      WorldSurface: worldSurfaceY,
      surfaceRandomOffset,
      request.Overwrite);
    if (LegacyTileRunnerOverridePolicy.MustPreserveExistingTile(context))
    {
      return null;
    }

    return new TileChangeCommand(
      sequence,
      x,
      y,
      TileChangeKind.UpdateTileType,
      checked((ushort)request.TileType),
      Source: source,
      IsActive: request.AddTile ? true : current.IsActive);
  }

  private static string CreateSource(LegacyTileRunnerPassInvocation invocation)
  {
    return $"worldgen.cave.{invocation.Recipe.PassName}.{invocation.Recipe.RecipeName}";
  }

  private static bool ShouldWriteDirtWall(
    LegacyTileRunnerRequest request,
    int y,
    int worldSurfaceY)
  {
    return request.NoYChange && y < worldSurfaceY && request.TileType != 59;
  }
}
