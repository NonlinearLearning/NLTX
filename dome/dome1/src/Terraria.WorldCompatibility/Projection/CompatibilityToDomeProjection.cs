using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects;
using Terraria.Dome.Simulation.Npc.Snapshots;
using Terraria.WorldCompatibility.Model;

namespace Terraria.WorldCompatibility.Projection;

public static class CompatibilityToDomeProjection
{
  private const int EndlessRainRepairLastAffectedVersion = 317;
  private const int EndlessRainRepairThresholdTicks = 5184000;

  public static DomeSimulationSnapshot Project(
    CompatibilityWorldSnapshot snapshot,
    WorldSeed seed,
    bool strictImport = true,
    bool? isRainsForAYearSecretSeedActive = null)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (strictImport && HasRequiredUnsupportedRecords(snapshot.LoadReport))
    {
      throw new InvalidOperationException(
        "Strict compatibility import rejected unsupported world state.");
    }

    WorldMetadata metadata = new(
      snapshot.Metadata.Name,
      seed,
      snapshot.Metadata.Width,
      snapshot.Metadata.Height,
      snapshot.Metadata.WorldId,
      snapshot.Metadata.SpawnX,
      snapshot.Metadata.SpawnY,
      worldSurface: snapshot.Metadata.WorldSurface,
      rockLayer: snapshot.Metadata.RockLayer,
      isRemixWorld: snapshot.Metadata.IsRemixWorld,
      worldGeneratorVersion: snapshot.Metadata.WorldGeneratorVersion,
      uniqueId: snapshot.Metadata.UniqueId,
      seedText: snapshot.Metadata.SeedText,
      isNoTrapsWorld: snapshot.Metadata.IsNoTrapsWorld,
      isSkyblockWorld: snapshot.Metadata.IsSkyblockWorld,
      isGoodWorld: snapshot.Metadata.IsGoodWorld);
    WorldGrid world = new(metadata.Width, metadata.Height);
    for (int index = 0; index < snapshot.Tiles.Count; index++)
    {
      CompatibilityTile source = snapshot.Tiles[index];
      if (!world.TrySetTile(source.X, source.Y, new WorldTile(
        IsActive: source.IsActive,
        Type: source.TileType,
        LiquidAmount: source.LiquidAmount,
        LiquidType: source.LiquidKind,
        FrameX: source.FrameX,
        FrameY: source.FrameY,
        WallType: source.WallType,
        HasWire: source.HasWire,
        HasWire2: source.HasWire2,
        HasWire3: source.HasWire3,
        HasWire4: source.HasWire4,
        IsHalfBrick: source.IsHalfBrick,
        Slope: source.Slope,
        IsActuated: source.IsActuated,
        IsInactive: source.IsInactive,
        TileColor: source.TileColor,
        WallColor: source.WallColor,
        IsInvisibleBlock: source.IsInvisibleBlock,
        IsInvisibleWall: source.IsInvisibleWall,
        IsFullbrightBlock: source.IsFullbrightBlock,
        IsFullbrightWall: source.IsFullbrightWall)))
      {
        throw new InvalidOperationException("A compatibility tile was outside the Dome world.");
      }
    }

    WorldGridSnapshot worldSnapshot = world.CreateSnapshot(metadata);
    List<NpcReplicationSnapshot> npcs = new(snapshot.Npcs.Count);
    List<NpcStateSnapshot> npcStates = new(snapshot.Npcs.Count);
    for (int index = 0; index < snapshot.Npcs.Count; index++)
    {
      CompatibilityNpcSnapshot source = snapshot.Npcs[index];
      NpcReplicationSnapshot replication = new(
        index + 1,
        source.Type,
        new SimulationVector(source.PositionX, source.PositionY),
        new SimulationVector(0.0f, 0.0f),
        100,
        true,
        1,
        GetSection(source.PositionX, source.PositionY),
        DefinitionId: source.Type,
        MaximumHealth: 100);
      npcs.Add(replication);
      npcStates.Add(NpcStateSnapshot.FromReplication(replication) with
      {
        GivenName = source.Name
      });
    }

    List<ChestPersistentState> chests = new(snapshot.Chests.Count);
    for (int index = 0; index < snapshot.Chests.Count; index++)
    {
      CompatibilityChestSnapshot source = snapshot.Chests[index];
      ItemStack[] slots = new ItemStack[ChestComponent.SlotCount];
      int copyCount = Math.Min(source.Items.Count, slots.Length);
      for (int slot = 0; slot < copyCount; slot++)
      {
        CompatibilityChestItem item = source.Items[slot];
        if (item.Stack > 0 && item.NetId > 0 && item.NetId <= ushort.MaxValue)
        {
          slots[slot] = new ItemStack((ushort)item.NetId, item.Stack);
        }
      }

      if (source.Items.Count > slots.Length && strictImport)
      {
        throw new InvalidOperationException("Strict compatibility import rejected oversized chest slots.");
      }

      chests.Add(new ChestPersistentState(index + 1, source.X, source.Y, slots, 1, source.Name));
    }

    List<SignPersistentState> signs = new(snapshot.Signs.Count);
    for (int index = 0; index < snapshot.Signs.Count; index++)
    {
      CompatibilitySignSnapshot source = snapshot.Signs[index];
      signs.Add(new SignPersistentState(index, source.X, source.Y, source.Text, 1));
    }

    List<TileEntityPersistentState> tileEntities = new(snapshot.TileEntities.Count);
    for (int index = 0; index < snapshot.TileEntities.Count; index++)
    {
      CompatibilityTileEntitySnapshot source = snapshot.TileEntities[index];
      tileEntities.Add(new TileEntityPersistentState(
        source.Id,
        source.Type,
        source.X,
        source.Y,
        source.Payload,
        source.IsOpaque));
    }

    List<OpaqueCompatibilityRecord> opaqueRecords = new(
      snapshot.LoadReport.UnsupportedRecords.Count);
    foreach (CompatibilityUnsupportedRecord source in snapshot.LoadReport.UnsupportedRecords)
    {
      opaqueRecords.Add(new OpaqueCompatibilityRecord(
        source.SectionIndex,
        source.Offset,
        source.Reason,
        source.Type,
        source.X,
        source.Y,
        source.Payload,
        source.IsRequired));
    }

    return new DomeSimulationSnapshot(
      worldSnapshot,
      npcs,
      [],
      0,
      npcStates: npcStates,
      worldClock: new WorldClockSnapshot(
        0,
        GetValidatedTimeOfDay(snapshot.Metadata),
        snapshot.Metadata.IsDayTime,
        false,
        1,
        MoonPhase: snapshot.Metadata.MoonPhase),
      chests: chests,
      signs: signs,
      tileEntities: tileEntities,
      opaqueCompatibilityRecords: opaqueRecords,
      worldRules: CreateWorldRules(snapshot, isRainsForAYearSecretSeedActive),
      progression: new WorldProgressionState(
        isHardMode: snapshot.Metadata.IsHardMode,
        defeatedEyeOfCthulhu: snapshot.Metadata.DefeatedEyeOfCthulhu,
        defeatedEaterOrBrain: snapshot.Metadata.DefeatedEaterOrBrain,
        defeatedSkeletron: snapshot.Metadata.DefeatedSkeletron,
        defeatedMechanicalBoss: snapshot.Metadata.DefeatedMechanicalBoss,
        defeatedPlantera: snapshot.Metadata.DefeatedPlantera,
        defeatedGolem: snapshot.Metadata.DefeatedGolem,
        invasionType: snapshot.Metadata.InvasionType,
        invasionSize: snapshot.Metadata.InvasionSize,
        invasionX: snapshot.Metadata.InvasionX,
        defeatedGoblins: snapshot.Metadata.DefeatedGoblins,
        defeatedFrost: snapshot.Metadata.DefeatedFrost,
        defeatedPirates: snapshot.Metadata.DefeatedPirates,
        defeatedMartians: snapshot.Metadata.DefeatedMartians,
        isMeteorScheduled: snapshot.Metadata.IsMeteorScheduled,
        isBloodMoon: snapshot.Metadata.IsBloodMoon,
        isEclipse: snapshot.Metadata.IsEclipse));
  }

  private static WorldSectionCoordinates GetSection(float x, float y)
  {
    return new WorldSectionCoordinates(
      Math.Max(0, (int)x / WorldGrid.SectionWidth),
      Math.Max(0, (int)y / WorldGrid.SectionHeight));
  }

  private static double GetValidatedTimeOfDay(CompatibilityWorldMetadata metadata)
  {
    try
    {
      WorldClock clock = new(
        timeOfDay: metadata.TimeOfDay,
        isDayTime: metadata.IsDayTime);
      return clock.TimeOfDay;
    }
    catch (ArgumentException exception)
    {
      throw new InvalidOperationException(
        "Compatibility import rejected an invalid WLD world time.",
        exception);
    }
  }

  private static WorldRuleState CreateWorldRules(
    CompatibilityWorldSnapshot snapshot,
    bool? isRainsForAYearSecretSeedActive)
  {
    CompatibilityWorldMetadata metadata = snapshot.Metadata;
    bool hasRainFacts = metadata.IsRaining.HasValue ||
      metadata.RainTimeTicks.HasValue ||
      metadata.MaximumRainStrength.HasValue;
    if (hasRainFacts && (!metadata.IsRaining.HasValue || !metadata.RainTimeTicks.HasValue ||
                         !metadata.MaximumRainStrength.HasValue))
    {
      throw new InvalidOperationException(
        "Compatibility import rejected an incomplete WLD rain-state triple.");
    }

    bool requiresEndlessRainRepair = hasRainFacts &&
      snapshot.Version <= EndlessRainRepairLastAffectedVersion &&
      metadata.RainTimeTicks >= EndlessRainRepairThresholdTicks;
    if (requiresEndlessRainRepair && !isRainsForAYearSecretSeedActive.HasValue)
    {
      throw new InvalidOperationException(
        "Compatibility import rejected a legacy rain repair without secret-seed context.");
    }

    bool isRainsForAYearActive = isRainsForAYearSecretSeedActive.GetValueOrDefault();
    if (requiresEndlessRainRepair && !isRainsForAYearActive)
    {
      return new WorldRuleState(
        difficulty: GetBaseDifficulty(metadata.GameMode),
        isExpertMode: metadata.GameMode is 1 or 2,
        isMasterMode: metadata.GameMode == 2,
        isCrimsonWorld: metadata.IsCrimsonWorld,
        gameMode: GetGameMode(metadata.GameMode),
        windSpeedTarget: metadata.WindSpeedTarget ?? 0.0f,
        windSpeedCurrent: metadata.WindSpeedTarget ?? 0.0f);
    }

    return new WorldRuleState(
      difficulty: GetBaseDifficulty(metadata.GameMode),
      isExpertMode: metadata.GameMode is 1 or 2,
      isMasterMode: metadata.GameMode == 2,
      isCrimsonWorld: metadata.IsCrimsonWorld,
      rainTimeTicks: metadata.RainTimeTicks ?? 0,
      gameMode: GetGameMode(metadata.GameMode),
      isRaining: metadata.IsRaining,
      maximumRainStrength: metadata.MaximumRainStrength,
      windSpeedTarget: metadata.WindSpeedTarget ?? 0.0f,
      windSpeedCurrent: metadata.WindSpeedTarget ?? 0.0f);
  }

  private static int GetBaseDifficulty(int gameMode)
  {
    return gameMode switch
    {
      0 => 0,
      1 => 1,
      2 => 2,
      3 => 0,
      _ => throw new ArgumentOutOfRangeException(
        nameof(gameMode),
        "The WLD game mode is outside the supported range.")
    };
  }

  private static WorldGameMode GetGameMode(int gameMode)
  {
    return gameMode switch
    {
      0 => WorldGameMode.Classic,
      1 => WorldGameMode.Expert,
      2 => WorldGameMode.Master,
      3 => WorldGameMode.Journey,
      _ => throw new ArgumentOutOfRangeException(
        nameof(gameMode),
        "The WLD game mode is outside the supported range.")
    };
  }

  private static bool HasRequiredUnsupportedRecords(CompatibilityLoadReport report)
  {
    foreach (CompatibilityUnsupportedRecord record in report.UnsupportedRecords)
    {
      if (record.IsRequired)
      {
        return true;
      }
    }

    return false;
  }
}
