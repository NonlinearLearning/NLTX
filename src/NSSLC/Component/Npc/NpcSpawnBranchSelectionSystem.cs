using System;

namespace Terraria.Npc;

public static class NpcSpawnBranchSelectionSystem
{
  private const int NormalPostSpawnTimeLeftMultiplier = 1;
  private const int StatueMimicType = 690;
  private const int Type244WallType = 244;

  public static NpcSpawnBranchSelectionResult Select(
    in NpcSpawnBranchSelectionInputs inputs,
    INpcSpawnTowerSelectionPort towerPort,
    INpcSpawnSkyMobSelectionPort skyMobPort,
    INpcSpawnInvasionSelectionPort invasionPort,
    INpcSpawnGraveyardDualDungeonSelectionPort graveyardPort,
    INpcSpawnCritterSelectionPort critterPort)
  {
    ArgumentNullException.ThrowIfNull(towerPort);
    ArgumentNullException.ThrowIfNull(skyMobPort);
    ArgumentNullException.ThrowIfNull(invasionPort);
    ArgumentNullException.ThrowIfNull(graveyardPort);
    ArgumentNullException.ThrowIfNull(critterPort);

    NpcSpawnAcceptedCandidate candidate = inputs.Candidate;
    NpcSpawnTileSearchResult tile = candidate.TileSearchResult;
    if (!tile.Found)
    {
      return CreateUnmatchedResult();
    }

    int tileX = tile.TileX;
    int tileY = tile.TileY;
    NpcSpawnTowerSelectionInputs towerInputs = inputs.Tower with
    {
      SpawnTileX = tileX,
      SpawnTileY = tileY,
    };
    NpcSpawnTowerSelectionResult towerResult =
      NpcSpawnTowerSelectionSystem.Select(in towerInputs, towerPort);
    if (towerResult.Status == NpcSpawnTowerSelectionStatus.RequestProduced)
    {
      return CreateRequestResult(
        NpcSpawnBranchKind.Tower,
        towerResult.SpawnRequest);
    }

    NpcSpawnSkyMobSelectionInputs skyMobInputs = inputs.SkyMob with
    {
      IsSkyMob = tile.SkyMob,
      SpawnTileX = tileX,
      SpawnTileY = tileY,
      Invaders = inputs.Invasion.Invaders,
      InvasionType = inputs.Invasion.InvasionType,
      NoWorms = candidate.NoWormsForSpawn,
    };
    NpcSpawnSkyMobSelectionResult skyMobResult =
      NpcSpawnSkyMobSelectionSystem.Select(in skyMobInputs, skyMobPort);
    if (skyMobResult.Status == NpcSpawnSkyMobSelectionStatus.RequestProduced)
    {
      return CreateRequestResult(
        NpcSpawnBranchKind.SkyMob,
        skyMobResult.SpawnRequest);
    }

    NpcSpawnInvasionSelectionInputs invasionInputs = inputs.Invasion with
    {
      SpawnTileX = tileX,
      SpawnTileY = tileY,
    };
    NpcSpawnInvasionSelectionResult invasionResult =
      NpcSpawnInvasionSelectionSystem.Select(in invasionInputs, invasionPort);
    if (invasionResult.Status == NpcSpawnInvasionSelectionStatus.UnknownInvasionTypeEarlyReturn)
    {
      return new NpcSpawnBranchSelectionResult(
        NpcSpawnBranchSelectionStatus.UnknownInvasionTypeEarlyReturn,
        NpcSpawnBranchKind.Invasion,
        null,
        NormalPostSpawnTimeLeftMultiplier);
    }

    if (invasionResult.Status == NpcSpawnInvasionSelectionStatus.HandledWithoutRequest)
    {
      return new NpcSpawnBranchSelectionResult(
        NpcSpawnBranchSelectionStatus.HandledWithoutRequest,
        NpcSpawnBranchKind.Invasion,
        null,
        NormalPostSpawnTimeLeftMultiplier);
    }

    if (invasionResult.Status == NpcSpawnInvasionSelectionStatus.RequestProduced)
    {
      return CreateRequestResult(
        NpcSpawnBranchKind.Invasion,
        invasionResult.SpawnRequest);
    }

    // The legacy biome/event chain between invasion and graveyard is not represented here.
    if (inputs.EarlierUnmodeledBranches !=
      NpcSpawnEarlierLegacyBranchDisposition.NoBranchMatched)
    {
      return new NpcSpawnBranchSelectionResult(
        NpcSpawnBranchSelectionStatus.EarlierUnmodeledBranchesUnresolved,
        null,
        null,
        NormalPostSpawnTimeLeftMultiplier);
    }

    NpcSpawnGraveyardDualDungeonSelectionResult graveyardResult =
      NpcSpawnGraveyardDualDungeonSelectionSystem.Select(
        in candidate,
        graveyardPort);
    if (graveyardResult.Status == NpcSpawnGraveyardDualDungeonSelectionStatus.RequestProduced)
    {
      NpcSpawnEntityRequest request = graveyardResult.SpawnRequest ??
        throw new InvalidOperationException(
          "A selected graveyard or dual-dungeon branch must include its pre-commit request.");
      NpcSpawnBranchKind branch = request.Type.Value == StatueMimicType
        ? NpcSpawnBranchKind.StatueMimic
        : NpcSpawnBranchKind.DualDungeon;
      return CreateRequestResult(branch, request);
    }

    NpcSpawnLegacyWallClassificationFacts legacyWallFacts = inputs.LegacyWallFacts;
    int legacyWallType = NpcSpawnLegacyWallClassificationQuery.ResolveLegacyWallType(
      in legacyWallFacts);
    NpcSpawnCritterSelectionInputs critterInputs = inputs.Critter with
    {
      SpawnTileX = tileX,
      SpawnTileY = tileY,
    };
    if (legacyWallType != Type244WallType || critterInputs.RemixWorld)
    {
      return CreateUnmatchedResult();
    }

    NpcSpawnCritterSelectionResult critterResult =
      NpcSpawnCritterSelectionSystem.Select(in critterInputs, critterPort);
    if (critterResult.Status == NpcSpawnCritterSelectionStatus.HandledWithoutRequest)
    {
      return new NpcSpawnBranchSelectionResult(
        NpcSpawnBranchSelectionStatus.HandledWithoutRequest,
        NpcSpawnBranchKind.Type244Critter,
        null,
        critterResult.PostSpawnTimeLeftMultiplier);
    }

    if (critterResult.Status == NpcSpawnCritterSelectionStatus.RequestProduced)
    {
      return CreateRequestResult(
        NpcSpawnBranchKind.Type244Critter,
        critterResult.SpawnRequest,
        critterResult.PostSpawnTimeLeftMultiplier);
    }

    return CreateUnmatchedResult();
  }

  private static NpcSpawnBranchSelectionResult CreateRequestResult(
    NpcSpawnBranchKind branch,
    NpcSpawnEntityRequest? request,
    int postSpawnTimeLeftMultiplier = NormalPostSpawnTimeLeftMultiplier)
  {
    if (!request.HasValue)
    {
      throw new InvalidOperationException(
        "A selected NPC spawn branch must include its pre-commit request.");
    }

    return new NpcSpawnBranchSelectionResult(
      NpcSpawnBranchSelectionStatus.RequestProduced,
      branch,
      request,
      postSpawnTimeLeftMultiplier);
  }

  private static NpcSpawnBranchSelectionResult CreateUnmatchedResult()
  {
    return new NpcSpawnBranchSelectionResult(
      NpcSpawnBranchSelectionStatus.NoModeledBranchMatched,
      null,
      null,
      NormalPostSpawnTimeLeftMultiplier);
  }
}
