using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Players;
using Terraria.Dome.Simulation.Npc.Snapshots;
using Terraria.Dome.Simulation.WorldModel;

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
    WorldProgressionState? progression = null)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(npcs);
    ArgumentNullException.ThrowIfNull(worldItems);
    if (tickNumber < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tickNumber));
    }

    World = world;
    Npcs = [.. npcs];
    NpcStates = npcStates is null ? [] : new SnapshotReadOnlyList<NpcStateSnapshot>(npcStates);
    WorldItems = [.. worldItems];
    PlayerAccounts = playerAccounts is null ? [] : [.. playerAccounts];
    Chests = chests is null ? [] : new SnapshotReadOnlyList<ChestPersistentState>(chests);
    Signs = signs is null ? [] : new SnapshotReadOnlyList<SignPersistentState>(signs);
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
  }

  public IReadOnlyList<ChestPersistentState> Chests { get; }

  public WorldClockSnapshot Clock { get; }

  public WorldProgressionState Progression { get; }

  public WorldGridSnapshot World { get; }
  public IReadOnlyList<NpcReplicationSnapshot> Npcs { get; }
  public IReadOnlyList<NpcStateSnapshot> NpcStates { get; }
  public IReadOnlyList<OpaqueCompatibilityRecord> OpaqueCompatibilityRecords { get; }
  public IReadOnlyList<ItemReplicationSnapshot> WorldItems { get; }
  public IReadOnlyList<PlayerPersistentState> PlayerAccounts { get; }
  public IReadOnlyList<SignPersistentState> Signs { get; }
  public IReadOnlyList<TileEntityPersistentState> TileEntities { get; }
  public long TickNumber => Clock.TickNumber;
  public WorldRuleState WorldRules { get; }
}
