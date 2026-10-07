using System;
using System.Numerics;
using Terraria.Npc.Network;

namespace Terraria.Npc;

public static class NpcDeathWorldEffectSystem
{
  public static NpcDeathWorldEffectResult Apply(
    in NpcDeathWorldEffectIntent intent,
    INpcDeathWorldEffectPort worldPort,
    INpcSpawnSyncPacketPort syncPacketPort)
  {
    ArgumentNullException.ThrowIfNull(worldPort);
    ArgumentNullException.ThrowIfNull(syncPacketPort);
    ValidateIntent(in intent);

    int spawnCalls = 0;
    int syncRequests = 0;
    int tileSearchAttempts = 0;
    int exhaustedSearches = 0;
    int maxNpcSlots = worldPort.MaxNpcSlots;

    for (int spawnIndex = 0; spawnIndex < intent.SpawnCount; spawnIndex++)
    {
      if (intent.Kind == NpcDeathWorldEffectKind.FixedPositionSpawn)
      {
        Vector2 position = intent.FixedPositionPixels!.Value;
        int legacySlot = worldPort.SpawnNpcFromNpcAi(
          intent.SourceNpcInstanceId,
          intent.SpawnNpcType,
          (int)position.X,
          (int)position.Y);
        spawnCalls++;
        syncRequests += RequestSyncIfAccepted(
          legacySlot,
          maxNpcSlots,
          intent.RequestReplicationSyncAfterSpawn,
          syncPacketPort);
        continue;
      }

      bool found = false;
      for (int attempt = 0; attempt < intent.SearchAttemptsPerSpawn; attempt++)
      {
        tileSearchAttempts++;
        int tileX = (int)(intent.OriginCenter.X / 16.0f) + Next(
          worldPort,
          -intent.RandomOffsetRadiusInTiles,
          intent.RandomOffsetRadiusInTiles + 1);
        int tileY = (int)(intent.OriginCenter.Y / 16.0f) + Next(
          worldPort,
          -intent.RandomOffsetRadiusInTiles,
          intent.RandomOffsetRadiusInTiles + 1);
        int maxTileY = worldPort.MaxTilesY - intent.WorldBottomMarginInTiles;

        while (tileY < maxTileY && !worldPort.IsSolidTile(tileX, tileY))
        {
          tileY++;
        }

        tileY--;
        if (worldPort.IsSolidTile(tileX, tileY))
        {
          continue;
        }

        int legacySlot = worldPort.SpawnNpcFromNpcAi(
          intent.SourceNpcInstanceId,
          intent.SpawnNpcType,
          tileX * 16 + 8,
          tileY * 16);
        spawnCalls++;
        syncRequests += RequestSyncIfAccepted(
          legacySlot,
          maxNpcSlots,
          intent.RequestReplicationSyncAfterSpawn,
          syncPacketPort);
        found = true;
        break;
      }

      if (!found)
      {
        exhaustedSearches++;
      }
    }

    return new NpcDeathWorldEffectResult(
      spawnCalls,
      syncRequests,
      tileSearchAttempts,
      exhaustedSearches);
  }

  private static void ValidateIntent(in NpcDeathWorldEffectIntent intent)
  {
    if (!intent.SourceNpcInstanceId.IsValid)
    {
      throw new ArgumentException(
        "A death world effect requires its source NPC identity.",
        nameof(intent));
    }

    if (intent.SpawnCount < 0 || intent.RandomOffsetRadiusInTiles < 0 ||
        intent.SearchAttemptsPerSpawn < 0 || intent.WorldBottomMarginInTiles < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(intent));
    }

    if (intent.Kind == NpcDeathWorldEffectKind.FixedPositionSpawn &&
        !intent.FixedPositionPixels.HasValue)
    {
      throw new ArgumentException(
        "A fixed-position death effect requires a position.",
        nameof(intent));
    }

    if (intent.Kind == NpcDeathWorldEffectKind.SurfaceSearchSpawn &&
        intent.SearchAttemptsPerSpawn > 0 &&
        intent.RandomOffsetRadiusInTiles == int.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(intent));
    }

    if (intent.Kind is not NpcDeathWorldEffectKind.FixedPositionSpawn and
        not NpcDeathWorldEffectKind.SurfaceSearchSpawn)
    {
      throw new ArgumentOutOfRangeException(nameof(intent));
    }
  }

  private static int Next(
    INpcDeathWorldEffectPort port,
    int minimumInclusive,
    int maximumExclusive)
  {
    int value = port.Next(minimumInclusive, maximumExclusive);
    if (value < minimumInclusive || value >= maximumExclusive)
    {
      throw new InvalidOperationException(
        "The death effect random port returned a value outside its requested range.");
    }

    return value;
  }

  private static int RequestSyncIfAccepted(
    int legacySlot,
    int maxNpcSlots,
    bool requestSync,
    INpcSpawnSyncPacketPort syncPacketPort)
  {
    if (!requestSync || legacySlot >= maxNpcSlots)
    {
      return 0;
    }

    syncPacketPort.SendNpcSyncPacket(legacySlot);
    return 1;
  }
}
