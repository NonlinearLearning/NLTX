using System;
using System.Collections.Generic;
using System.Threading;
using NSSLC.WorldGeneration;
using NSSLC.WorldGeneration.WorldBuilding;
using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldSession.Components;
using Terraria.WorldStorage;
using StorageTileCoordinate = global::Terraria.WorldStorage.TileCoordinate;

namespace Terraria.NonAuthoritative.WorldStorage;

/// <summary>
/// Adapts legacy load recovery while requiring the host to provide session publication,
/// additional cleanup, and reset.
/// </summary>
public sealed class LegacyWorldLoadRecoveryEffects : IWorldLoadRecoveryEffects
{
  private readonly Func<LoadedWorldSession, CancellationToken, WorldStorageOperationResult>
    _publishLoadedWorld;
  private readonly Func<LoadedWorldSession, CancellationToken, WorldStorageOperationResult>
    _preparePreReleaseWeather;
  private readonly Func<LoadedWorldSession, CancellationToken, WorldStorageOperationResult>
    _finalizeLoadedWorld;
  private readonly Func<LoadedWorldSession, WorldStorageOperationResult> _resetWorld;

  /// <param name="preparePreReleaseWeather">
  /// Performs host-specific cleanup after the legacy weather counter and cloud reset, before the
  /// final water check.
  /// </param>
  /// <param name="finalizeLoadedWorld">
  /// Performs host-specific finalization after the legacy NPC and world-derived initialization.
  /// </param>
  public LegacyWorldLoadRecoveryEffects(
    Func<LoadedWorldSession, CancellationToken, WorldStorageOperationResult> publishLoadedWorld,
    Func<LoadedWorldSession, CancellationToken, WorldStorageOperationResult>
      preparePreReleaseWeather,
    Func<LoadedWorldSession, CancellationToken, WorldStorageOperationResult>
      finalizeLoadedWorld,
    Func<LoadedWorldSession, WorldStorageOperationResult> resetWorld)
  {
    _publishLoadedWorld = publishLoadedWorld ??
      throw new ArgumentNullException(nameof(publishLoadedWorld));
    _preparePreReleaseWeather = preparePreReleaseWeather ??
      throw new ArgumentNullException(nameof(preparePreReleaseWeather));
    _finalizeLoadedWorld = finalizeLoadedWorld ??
      throw new ArgumentNullException(nameof(finalizeLoadedWorld));
    _resetWorld = resetWorld ?? throw new ArgumentNullException(nameof(resetWorld));
  }

  public WorldStorageOperationResult PublishLoadedWorld(
    LoadedWorldSession session,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(session);
    if (cancellationToken.IsCancellationRequested)
    {
      return Canceled("World publication was canceled.");
    }

    WorldStorageOperationResult legacyTileEntityMigration =
      LegacyWorldTileEntityMigration.Apply(session, cancellationToken);
    if (!legacyTileEntityMigration.Succeeded ||
        legacyTileEntityMigration.Failure.Kind != WorldStorageFailureKind.None)
    {
      return legacyTileEntityMigration;
    }

    WorldStorageOperationResult publishResult = _publishLoadedWorld(session, cancellationToken);
    if (!publishResult.Succeeded || publishResult.Failure.Kind != WorldStorageFailureKind.None)
    {
      return publishResult;
    }

    if (cancellationToken.IsCancellationRequested)
    {
      return Canceled("World publication was canceled after the host commit.");
    }

    WorldStorageOperationResult runtimeWorldValidation = ValidatePublishedRuntimeWorld(session);
    if (!runtimeWorldValidation.Succeeded)
    {
      return runtimeWorldValidation;
    }

    WorldStorageOperationResult oreTierRecovery = RecoverMissingSavedOreTiers(session);
    if (!oreTierRecovery.Succeeded ||
        oreTierRecovery.Failure.Kind != WorldStorageFailureKind.None)
    {
      return oreTierRecovery;
    }

    if (cancellationToken.IsCancellationRequested)
    {
      return Canceled("World publication was canceled after saved ore-tier recovery.");
    }

    WorldStorageOperationResult tileLoadRepair =
      ApplyLegacyTileLoadRepairs(session, cancellationToken);
    if (!tileLoadRepair.Succeeded ||
        tileLoadRepair.Failure.Kind != WorldStorageFailureKind.None)
    {
      return tileLoadRepair;
    }

    WorldGen.CaptureWorldDimensionCompatibilityState(
      session.DimensionCompatibility.LastMaxTilesX,
      session.DimensionCompatibility.LastMaxTilesY);
    return WorldStorageOperationResult.Success;
  }

  public WorldStorageOperationResult SettleLoadedWorld(
    LoadedWorldSession session,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(session);
    if (!session.IsPublished)
    {
      return WorldStorageOperationResult.Failed(
        WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "Liquid settling requires a published world session."));
    }

    WorldStorageOperationResult runtimeWorldValidation = ValidatePublishedRuntimeWorld(session);
    if (!runtimeWorldValidation.Succeeded)
    {
      return runtimeWorldValidation;
    }

    if (cancellationToken.IsCancellationRequested)
    {
      return Canceled("Liquid settling was canceled before it began.");
    }

    var changedCoordinates = new HashSet<StorageTileCoordinate>();
    WorldStorageOperationResult bindResult = LegacyWorldTileMapProjection.BindMutationObserver(
      session,
      (x, y) => changedCoordinates.Add(new StorageTileCoordinate(x, y)));
    if (!bindResult.Succeeded)
    {
      return bindResult;
    }

    WorldStorageOperationResult settleResult;
    WorldStorageOperationResult unbindResult;
    try
    {
      settleResult = SettleLoadedWorldCore(session, cancellationToken, changedCoordinates);
    }
    finally
    {
      Liquid.quickSettle = false;
      unbindResult = LegacyWorldTileMapProjection.UnbindMutationObserver(session);
    }

    return unbindResult.Succeeded ? settleResult : unbindResult;
  }

  private WorldStorageOperationResult SettleLoadedWorldCore(
    LoadedWorldSession session,
    CancellationToken cancellationToken,
    HashSet<StorageTileCoordinate> changedCoordinates)
  {
    GenVars.waterLine = Main.maxTilesY;
    Liquid.QuickWater(2);
    WorldGen.WaterCheck();
    WorldStorageOperationResult tileCommit = CommitSettledTileChanges(
      session,
      changedCoordinates);
    if (!tileCommit.Succeeded)
    {
      return tileCommit;
    }

    if (cancellationToken.IsCancellationRequested)
    {
      return Canceled("Liquid settling was canceled after the initial water check.");
    }

    int iteration = 0;
    int previousLiquidCount = Liquid.numLiquid + LiquidBuffer.numLiquidBuffer;
    float maximumProgress = 0f;
    Liquid.quickSettle = true;
    while (Liquid.numLiquid > 0 && iteration < 100000)
    {
      if (cancellationToken.IsCancellationRequested)
      {
        return Canceled("Liquid settling was canceled.");
      }

      iteration++;
      int currentLiquidCount = Liquid.numLiquid + LiquidBuffer.numLiquidBuffer;
      float progress = (float)(previousLiquidCount - currentLiquidCount) /
        previousLiquidCount;
      if (currentLiquidCount > previousLiquidCount)
      {
        previousLiquidCount = currentLiquidCount;
      }

      if (progress > maximumProgress)
      {
        maximumProgress = progress;
      }
      else
      {
        progress = maximumProgress;
      }

      Main.statusText = Lang.gen[27].Value + " " +
        (int)(progress * 100f / 2f + 50f) + "%";
      Liquid.UpdateLiquid();
      tileCommit = CommitSettledTileChanges(session, changedCoordinates);
      if (!tileCommit.Succeeded)
      {
        return tileCommit;
      }
    }

    if (cancellationToken.IsCancellationRequested)
    {
      return Canceled("Liquid settling was canceled before world cleanup.");
    }

    Main.weatherCounter = WorldGen.genRand.Next(3600, 18000);
    Cloud.resetClouds();

    WorldStorageOperationResult preReleaseResult =
      _preparePreReleaseWeather(session, cancellationToken);
    if (!preReleaseResult.Succeeded ||
        preReleaseResult.Failure.Kind != WorldStorageFailureKind.None)
    {
      return preReleaseResult;
    }

    tileCommit = CommitSettledTileChanges(session, changedCoordinates);
    if (!tileCommit.Succeeded)
    {
      return tileCommit;
    }

    if (cancellationToken.IsCancellationRequested)
    {
      return Canceled(
        "Liquid settling was canceled after pre-release weather cleanup and before the final water check.");
    }

    WorldGen.WaterCheck();
    tileCommit = CommitSettledTileChanges(session, changedCoordinates);
    if (!tileCommit.Succeeded)
    {
      return tileCommit;
    }

    return cancellationToken.IsCancellationRequested
      ? Canceled("Liquid settling was canceled after the final water check.")
      : WorldStorageOperationResult.Success;
  }

  private static WorldStorageOperationResult CommitSettledTileChanges(
    LoadedWorldSession session,
    HashSet<StorageTileCoordinate> changedCoordinates)
  {
    if (changedCoordinates.Count == 0)
    {
      return WorldStorageOperationResult.Success;
    }

    WorldStorageOperationResult result =
      LegacyWorldTileMapProjection.CommitLegacyTileMutations(session, changedCoordinates);
    if (result.Succeeded && result.Failure.Kind == WorldStorageFailureKind.None)
    {
      changedCoordinates.Clear();
    }
    return result;
  }

  public WorldStorageOperationResult FinalizeLoadedWorld(
    LoadedWorldSession session,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(session);
    if (cancellationToken.IsCancellationRequested)
    {
      return Canceled("World finalization was canceled.");
    }

    NPC.setFireFlyChance();
    if (cancellationToken.IsCancellationRequested)
    {
      return Canceled("World finalization was canceled after NPC spawn initialization.");
    }

    WorldGen.Skyblock.ScanTiles();
    if (cancellationToken.IsCancellationRequested)
    {
      return Canceled("World finalization was canceled after the Skyblock tile scan.");
    }

    if (Main.slimeRainTime > 0.0)
    {
      Main.RestoreSlimeRainAfterWorldLoad();
      if (cancellationToken.IsCancellationRequested)
      {
        return Canceled("World finalization was canceled after slime-rain restoration.");
      }
    }

    NPC.SetWorldSpecificMonstersByWorldID();
    if (cancellationToken.IsCancellationRequested)
    {
      return Canceled("World finalization was canceled after world-specific NPC setup.");
    }

    return _finalizeLoadedWorld(session, cancellationToken);
  }

  public WorldStorageOperationResult ResetWorld(LoadedWorldSession session)
  {
    ArgumentNullException.ThrowIfNull(session);
    return _resetWorld(session);
  }

  private static WorldStorageOperationResult Canceled(string detail)
  {
    return WorldStorageOperationResult.Failed(
      WorldStorageFailure.Create(WorldStorageFailureKind.Canceled, detail));
  }

  private static WorldStorageOperationResult ValidatePublishedRuntimeWorld(
    LoadedWorldSession session)
  {
    int expectedWidth = session.DimensionCompatibility.LastMaxTilesX;
    int expectedHeight = session.DimensionCompatibility.LastMaxTilesY;
    int descriptorWidth = session.World.Descriptor.SizeX;
    int descriptorHeight = session.World.Descriptor.SizeY;
    int actualWidth = Main.tile.GetLength(0);
    int actualHeight = Main.tile.GetLength(1);
    if (!session.DimensionCompatibility.HasPreviousDimensions ||
        expectedWidth != descriptorWidth || expectedHeight != descriptorHeight ||
        Main.maxTilesX != expectedWidth || Main.maxTilesY != expectedHeight ||
        actualWidth != expectedWidth || actualHeight != expectedHeight)
    {
      return WorldStorageOperationResult.Failed(
        WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          $"The published runtime world ({Main.maxTilesX}x{Main.maxTilesY}, " +
          $"tile array {actualWidth}x{actualHeight}) does not match the loaded session " +
          $"({descriptorWidth}x{descriptorHeight}, " +
          $"compatibility snapshot {expectedWidth}x{expectedHeight})."));
    }

    return WorldStorageOperationResult.Success;
  }

  private static WorldStorageOperationResult RecoverMissingSavedOreTiers(
    LoadedWorldSession session)
  {
    OreTierState savedOreTiers = session.World.Rules.SavedOreTiers;
    if (savedOreTiers.Copper != -1 &&
        savedOreTiers.Iron != -1 &&
        savedOreTiers.Silver != -1 &&
        savedOreTiers.Gold != -1)
    {
      return WorldStorageOperationResult.Success;
    }

    int[] oreCounts = WorldGen.CountTileTypesInWorld(7, 166, 6, 167, 9, 168, 8, 169);
    OreTierState recoveredOreTiers = new(
      Copper: oreCounts[0] > oreCounts[1] ? 7 : 166,
      Iron: oreCounts[2] > oreCounts[3] ? 6 : 167,
      Silver: oreCounts[4] > oreCounts[5] ? 9 : 168,
      Gold: oreCounts[6] > oreCounts[7] ? 8 : 169,
      Cobalt: savedOreTiers.Cobalt,
      Mythril: savedOreTiers.Mythril,
      Adamantite: savedOreTiers.Adamantite);
    session.World.Rules.SavedOreTiers = recoveredOreTiers;
    WorldGen.SavedOreTiers.Copper = recoveredOreTiers.Copper;
    WorldGen.SavedOreTiers.Iron = recoveredOreTiers.Iron;
    WorldGen.SavedOreTiers.Silver = recoveredOreTiers.Silver;
    WorldGen.SavedOreTiers.Gold = recoveredOreTiers.Gold;
    return WorldStorageOperationResult.Success;
  }

  private static WorldStorageOperationResult ApplyLegacyTileLoadRepairs(
    LoadedWorldSession session,
    CancellationToken cancellationToken)
  {
    var changedCoordinates = new HashSet<StorageTileCoordinate>();
    WorldStorageOperationResult bindResult = LegacyWorldTileMapProjection
      .BindCandidateMutationObserver(session, (x, y) =>
        changedCoordinates.Add(new StorageTileCoordinate(x, y)));
    if (!bindResult.Succeeded)
    {
      return bindResult;
    }

    WorldStorageOperationResult cleanupResult = WorldStorageOperationResult.Success;
    WorldStorageOperationResult unbindResult;
    try
    {
      for (int x = 0; x < Main.maxTilesX; x++)
      {
        if (cancellationToken.IsCancellationRequested)
        {
          cleanupResult = Canceled("Temporary tile cleanup was canceled.");
          break;
        }

        for (int y = 0; y < Main.maxTilesY; y++)
        {
          Tile tile = Main.tile[x, y];
          if (tile.type == 49 && (tile.frameX == -1 || tile.frameY == -1))
          {
            tile.frameX = 0;
            tile.frameY = 0;
            changedCoordinates.Add(new StorageTileCoordinate(x, y));
          }

          if (tile.type == 127 || tile.type == 504)
          {
            changedCoordinates.Add(new StorageTileCoordinate(x, y));
            WorldGen.KillTile(x, y);
          }
        }
      }

      if (cleanupResult.Succeeded && cancellationToken.IsCancellationRequested)
      {
        cleanupResult = Canceled("Temporary tile cleanup was canceled after the final column.");
      }

      if (cleanupResult.Succeeded)
      {
        cleanupResult = LegacyWorldTileMapProjection.CommitCandidateLegacyTileMutations(
          session,
          changedCoordinates);
      }
    }
    finally
    {
      unbindResult = LegacyWorldTileMapProjection.UnbindMutationObserver(session);
    }

    return unbindResult.Succeeded ? cleanupResult : unbindResult;
  }
}
