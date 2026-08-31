using System;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerCommandBatch(
  TileChangeCommand? TileCommand,
  LiquidChangeCommand? LiquidCommand,
  bool SetsLava);

public static class LegacyTileRunnerCommandEmitter
{
  public static LegacyTileRunnerCommandBatch Create(
    int x,
    int y,
    long tileSequence,
    long liquidSequence,
    int tileType,
    bool addTile,
    bool noYChange,
    bool tileIsActive,
    int waterLine,
    int lavaLine,
    double worldSurface,
    short liquidType,
    bool remixWorld,
    int rockLayer,
    int maxTilesY,
    bool isOceanDepth,
    byte liquidAmount = byte.MaxValue,
    byte currentLiquidAmount = 0)
  {
    LegacyTileRunnerSideEffectProfile sideEffects =
      LegacyTileRunnerSideEffectPolicy.Classify(
        tileType,
        addTile,
        noYChange,
        tileIsActive,
        y,
        waterLine,
        lavaLine,
        worldSurface,
        liquidType,
        remixWorld,
        rockLayer,
        maxTilesY,
        isOceanDepth,
        currentLiquidAmount);

    TileChangeCommand? tileCommand = null;
    if (sideEffects.ClearsActiveTile)
    {
      tileCommand = new TileChangeCommand(
        tileSequence,
        x,
        y,
        TileChangeKind.Kill,
        TileType: 0,
        PreserveLiquid: sideEffects.InjectsLiquidBeforeClear,
        Source: "worldgen.tile-runner");
    }
    else if (tileType >= 0)
    {
      tileCommand = new TileChangeCommand(
        tileSequence,
        x,
        y,
        TileChangeKind.Place,
        checked((ushort)tileType),
        WallType: sideEffects.WritesSurfaceWall ? (ushort)1 : (ushort)0,
        PreserveLiquid: false,
        Source: "worldgen.tile-runner");
    }

    LiquidChangeCommand? liquidCommand = null;
    if (sideEffects.InjectsLiquidBeforeClear)
    {
      LegacyTileRunnerLiquidProjection projection =
        LegacyTileRunnerLiquidProjectionPolicy.Resolve(
          sideEffects.InjectedLiquidType,
          sideEffects.SetsLava);

      liquidCommand = new LiquidChangeCommand(
        liquidSequence,
        x,
        y,
        liquidAmount,
        projection.CommandLiquidType,
        Source: "worldgen.tile-runner");
    }
    else if (sideEffects.ClearsLiquidOnActivation || sideEffects.ClearsLiquidForType59 ||
             sideEffects.ClearsLavaOnActivation)
    {
      liquidCommand = new LiquidChangeCommand(
        liquidSequence,
        x,
        y,
        0,
        0,
        Source: "worldgen.tile-runner");
    }

    return new LegacyTileRunnerCommandBatch(tileCommand, liquidCommand, sideEffects.SetsLava);
  }
}
