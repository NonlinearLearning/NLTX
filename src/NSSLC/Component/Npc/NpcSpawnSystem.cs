using System;
using System.Collections.Generic;
using Terraria.Npc.Queries;

namespace Terraria.Npc;

public sealed class NpcSpawnSystem
{
  private const int LegacyPlayerSlotCount = 255;

  private readonly INpcSpawnPassPort _port;

  public NpcSpawnSystem(INpcSpawnPassPort port)
  {
    _port = port ?? throw new ArgumentNullException(nameof(port));
  }

  public void ProcessEntry(INpcSpawnEntryPort entryPort)
  {
    _ = ProcessEntryDetailed(entryPort);
  }

  /// <summary>
  /// Runs the Version4 entry ordering and reports only effects observable through
  /// the current ports. A continuation request is legacy loop control; it is not
  /// an entity-creation result.
  /// </summary>
  public NpcSpawnEntryResult ProcessEntryDetailed(INpcSpawnEntryPort entryPort)
  {
    ArgumentNullException.ThrowIfNull(entryPort);

    if (entryPort.ConsumeNoSpawnCycle())
    {
      return new NpcSpawnEntryResult(
        NoSpawnCycleWasConsumed: true,
        RespawnCheckRan: false,
        Pass: new NpcSpawnPassResult(
          LoopControlPlayerIndex: null,
          CreationObservation: NpcSpawnCreationObservation.NotRequested));
    }

    entryPort.CheckRespawns();
    return new NpcSpawnEntryResult(
      NoSpawnCycleWasConsumed: false,
      RespawnCheckRan: true,
      Pass: ProcessNaturalSpawnPassDetailed());
  }

  /// <summary>
  /// Processes player slots in legacy order. The returned index indicates only that the
  /// legacy loop-control result was true; it does not report an NPC creation result.
  /// </summary>
  public int? ProcessNaturalSpawnPass()
  {
    return ProcessNaturalSpawnPassDetailed().LoopControlPlayerIndex;
  }

  /// <summary>
  /// Processes player slots in legacy order and preserves the distinction between
  /// a continuation request and an observed entity creation.
  /// </summary>
  public NpcSpawnPassResult ProcessNaturalSpawnPassDetailed()
  {
    for (var playerIndex = 0; playerIndex < LegacyPlayerSlotCount; playerIndex++)
    {
      NpcSpawnPlayerEligibilitySnapshot snapshot =
        _port.CapturePlayerEligibility(playerIndex);
      NpcSpawnPlayerEligibilityResult eligibility =
        NpcSpawnEligibilityQuery.Evaluate(in snapshot);
      if (!eligibility.IsEligible)
      {
        continue;
      }

      if (_port.IsSlimeRainActive)
      {
        _port.SpawnSlimeRainForPlayer(playerIndex);
      }

      NpcSpawnRateInputs rateInputs =
        NpcSpawnPerPlayerFlagsSystem.Prepare(playerIndex, _port);
      NpcSpawnRateResult rateResult =
        NpcSpawnRateSystem.Calculate(in rateInputs, _port);
      if (rateInputs.Player.NearbyActiveNpcSlots >= (float)rateResult.MaxSpawns)
      {
        continue;
      }

      if (_port.Next(rateResult.SpawnRate) != 0)
      {
        continue;
      }

      NpcSpawnAreaInputs areaInputs = _port.CaptureSpawnAreaInputs(playerIndex);
      NpcSpawnTileSearchResult tileSearchResult = NpcSpawnTileSearchSystem.Find(
        in areaInputs,
        in rateInputs,
        in rateResult,
        _port);
      if (!tileSearchResult.Found)
      {
        continue;
      }

      IReadOnlyList<NpcSpawnScreenPlayerSnapshot> screenPlayers =
        _port.CaptureScreenPlayers();
      NpcSpawnScreenExclusionInputs screenInputs = new(
        areaInputs.ScreenWidthPixels,
        areaInputs.ScreenHeightPixels,
        tileSearchResult.Area.SafeRangeX,
        tileSearchResult.Area.SafeRangeY,
        areaInputs.DualDungeonsSeed,
        screenPlayers);
      if (!NpcSpawnScreenExclusionQuery.IsSpawnTileOutsideScreen(
        tileSearchResult.TileX,
        tileSearchResult.TileY,
        in screenInputs))
      {
        continue;
      }

      NpcSpawnPostCheckInputs postCheckInputs = _port.CapturePostCheckInputs(
        playerIndex,
        in tileSearchResult);
      if (!NpcSpawnPostCheckSystem.IsAccepted(in postCheckInputs, _port))
      {
        continue;
      }

      NpcSpawnChosenTileWorldInputs chosenTileWorldInputs =
        _port.CaptureChosenTileWorldInputs(
          tileSearchResult.TileX,
          tileSearchResult.TileY,
          postCheckInputs.SpawnTileType);
      NpcSpawnChosenTileFlagsResult chosenTileFlags =
        NpcSpawnChosenTileFlagsSystem.Calculate(
          in rateInputs,
          in rateResult,
          in areaInputs,
          in tileSearchResult,
          in postCheckInputs,
          in chosenTileWorldInputs,
          _port);
      var candidate = new NpcSpawnAcceptedCandidate(
        playerIndex,
        rateInputs,
        rateResult,
        tileSearchResult,
        postCheckInputs,
        chosenTileFlags);
      _port.ContinueSpawnAttempt(in candidate);
      return new NpcSpawnPassResult(
        LoopControlPlayerIndex: playerIndex,
        CreationObservation: NpcSpawnCreationObservation.Unknown);
    }

    return new NpcSpawnPassResult(
      LoopControlPlayerIndex: null,
      CreationObservation: NpcSpawnCreationObservation.NotRequested);
  }
}
