using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Players;
using Terraria.Dome.Simulation.Npc.Snapshots;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects.Sign;

namespace Terraria.Dome.Simulation;

public sealed class DomeSimulationSnapshot
{
  public DomeSimulationSnapshot(
    WorldGridSnapshot world,
    IReadOnlyList<NpcReplicationSnapshot> npcs,
    IReadOnlyList<ItemReplicationSnapshot> worldItems,
    long tickNumber,
    IReadOnlyList<PlayerPersistentState>? playerAccounts = null,
    IReadOnlyList<ChestPersistentState>? chests = null,
    IReadOnlyList<SignPersistentState>? signs = null,
    IReadOnlyList<TileEntityPersistentState>? tileEntities = null,
    IReadOnlyList<OpaqueCompatibilityRecord>? opaqueCompatibilityRecords = null,
    WorldClockSnapshot? worldClock = null,
    IReadOnlyList<NpcStateSnapshot>? npcStates = null,
    WorldRuleState? worldRules = null,
    WorldProgressionState? progression = null,
    WorldEventRandomState? worldEventRandomState = null,
    WorldTimeRateSnapshot? worldTimeRate = null,
    IReadOnlyList<SignTombstoneSnapshot>? signTombstones = null,
    int nextProjectileIdentity = 1,
    long nextChestMutationSequence = 0,
    long nextLiquidSequence = 0,
    long nextWiringSequence = 0)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(npcs);
    ArgumentNullException.ThrowIfNull(worldItems);
    if (tickNumber < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tickNumber));
    }

    if (nextProjectileIdentity <= 0 || nextProjectileIdentity >= int.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(nextProjectileIdentity));
    }

    if (nextChestMutationSequence < 0 || nextChestMutationSequence >= long.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(nextChestMutationSequence));
    }

    if (nextLiquidSequence < 0 || nextLiquidSequence >= long.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(nextLiquidSequence));
    }

    if (nextWiringSequence < 0 || nextWiringSequence >= long.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(nextWiringSequence));
    }

    World = world;
    Npcs = [.. npcs];
    NpcStates = npcStates is null ? [] : new SnapshotReadOnlyList<NpcStateSnapshot>(npcStates);
    WorldItems = [.. worldItems];
    PlayerAccounts = playerAccounts is null ? [] : [.. playerAccounts];
    Chests = chests is null ? [] : new SnapshotReadOnlyList<ChestPersistentState>(chests);
    Signs = signs is null ? [] : new SnapshotReadOnlyList<SignPersistentState>(signs);
    SignTombstones = signTombstones is null
      ? []
      : new SnapshotReadOnlyList<SignTombstoneSnapshot>(signTombstones);
    TileEntities = tileEntities is null ? [] :
      new SnapshotReadOnlyList<TileEntityPersistentState>(tileEntities);
    OpaqueCompatibilityRecords = opaqueCompatibilityRecords is null ? [] :
      new SnapshotReadOnlyList<OpaqueCompatibilityRecord>(opaqueCompatibilityRecords);
    Clock = worldClock ?? new WorldClockSnapshot(tickNumber, 0, true, false, 1);
    if (Clock.TickNumber != tickNumber)
    {
      throw new ArgumentException(
        "The snapshot tick number must match the world clock tick number.",
        nameof(worldClock));
    }

    WorldRules = worldRules ?? new WorldRuleState();
    Progression = progression ?? new WorldProgressionState();
    WorldEventRandomState = worldEventRandomState ??
      new WorldEventRandomState(unchecked((uint)world.Metadata.Seed.Value));
    WorldTimeRate = worldTimeRate ?? WorldTimeRateSnapshot.Unavailable;
    NextProjectileIdentity = nextProjectileIdentity;
    NextChestMutationSequence = nextChestMutationSequence;
    NextLiquidSequence = nextLiquidSequence;
    NextWiringSequence = nextWiringSequence;
  }

  public IReadOnlyList<ChestPersistentState> Chests { get; }

  public WorldClockSnapshot Clock { get; }

  public WorldProgressionState Progression { get; }
  public WorldEventRandomState WorldEventRandomState { get; }
  public WorldTimeRateSnapshot WorldTimeRate { get; }
  public int NextProjectileIdentity { get; }
  public long NextChestMutationSequence { get; }
  public long NextLiquidSequence { get; }
  public long NextWiringSequence { get; }

  public WorldGridSnapshot World { get; }
  public IReadOnlyList<NpcReplicationSnapshot> Npcs { get; }
  public IReadOnlyList<NpcStateSnapshot> NpcStates { get; }
  public IReadOnlyList<OpaqueCompatibilityRecord> OpaqueCompatibilityRecords { get; }
  public IReadOnlyList<ItemReplicationSnapshot> WorldItems { get; }
  public IReadOnlyList<PlayerPersistentState> PlayerAccounts { get; }
  public IReadOnlyList<SignPersistentState> Signs { get; }

  public IReadOnlyList<SignTombstoneSnapshot> SignTombstones { get; }
  public IReadOnlyList<TileEntityPersistentState> TileEntities { get; }
  public long TickNumber => Clock.TickNumber;
  public WorldRuleState WorldRules { get; }
}
