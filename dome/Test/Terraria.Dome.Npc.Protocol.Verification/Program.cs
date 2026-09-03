using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Dome.Protocol.V1456.Npc;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.Npc.Snapshots;
using Terraria.Dome.Simulation.WorldModel;

using Dome = Terraria.Dome.Simulation.DomeSimulation;

using (Dome source = new())
{
  NpcHandle npc = source.CreateNpc(new SimulationVector(20.0f, 0.0f));
  IReadOnlyList<NpcStateSnapshot> states = source.CreateNpcStateSnapshots();
  NpcStateSnapshot state = states.Single(snapshot => snapshot.Replication.ReplicationId == npc.Value);
  if (state.DefinitionId <= 0 || state.NetId <= 0 || state.MaximumHealth != 100 ||
      state.Replication.Position != new SimulationVector(20.0f, 0.0f) ||
      state.Lifecycle.IsActive == false)
  {
    throw new InvalidOperationException("NPC state snapshot did not include the complete value state.");
  }

  WorldMetadata metadata = new("npc-protocol", new WorldSeed(12), 4200, 1200);
  DomeSimulationSnapshot persistence = source.CreatePersistenceSnapshot(metadata);
  NpcStateSnapshot namedState = state with { GivenName = "Guide" };
  using (Dome namedRestore = new(new DomeSimulationSnapshot(
           persistence.World,
           persistence.Npcs,
           persistence.WorldItems,
           persistence.TickNumber,
           npcStates: [namedState])))
  {
    if (namedRestore.CreateNpcStateSnapshots().Single().GivenName != "Guide")
    {
      throw new InvalidOperationException("NPC given name did not survive state snapshot restore.");
    }
  }

  if (persistence.NpcStates.Count != 1)
  {
    throw new InvalidOperationException("Dome persistence snapshot did not include NPC state snapshots.");
  }

  using Dome restored = new(persistence);
  NpcStateSnapshot restoredState = restored.CreateNpcStateSnapshots().Single();
  if (restoredState.Replication.ReplicationId != state.Replication.ReplicationId ||
      restoredState.Replication.Position != state.Replication.Position ||
      restoredState.MaximumHealth != state.MaximumHealth ||
      restoredState.Behavior.BehaviorId != state.Behavior.BehaviorId ||
      restoredState.Replication.Revision != state.Replication.Revision)
  {
    throw new InvalidOperationException("NPC state snapshot did not survive restore without entity identity.");
  }

  NpcStateSnapshot townState = state with
  {
    DefinitionId = Dome.FixtureNpcType,
    Replication = state.Replication with { ReplicationId = 2 },
    Behavior = new NpcBehaviorStateComponent(
      NpcBehaviorId.TownHome,
      state.Behavior.Chase,
      new NpcTownHomeState(new SimulationVector(8.0f, 3.0f), false, 12)),
    HasHome = true,
    Home = new NpcHomeComponent(8, 3, false, 12, townVariant: 1),
    Faction = NpcFaction.Town,
    Category = NpcCategory.Town
  };
  NpcStateSnapshot segmentState = state with
  {
    DefinitionId = 999,
    Replication = state.Replication with { ReplicationId = 3 },
    Behavior = new NpcBehaviorStateComponent(
      NpcBehaviorId.Segment,
      state.Behavior.Chase,
      state.Behavior.TownHome),
    HasSegment = true,
    Segment = new NpcSegmentComponent(
      new NpcHandle(3),
      default,
      default,
      segmentIndex: 0,
      isRoot: true,
      NpcSegmentLifePolicy.RootShared),
    Faction = NpcFaction.Hostile,
    Category = NpcCategory.Segment
  };
  DomeSimulationSnapshot compositionSnapshot = new(
    persistence.World,
    [townState.ToReplicationSnapshot(), segmentState.ToReplicationSnapshot()],
    persistence.WorldItems,
    persistence.TickNumber,
    worldClock: persistence.Clock,
    npcStates: [townState, segmentState]);
  using Dome restoredComposition = new(compositionSnapshot);
  NpcStateSnapshot restoredTown = restoredComposition.CreateNpcStateSnapshots()
    .Single(snapshot => snapshot.Replication.ReplicationId == 2);
  NpcStateSnapshot restoredSegment = restoredComposition.CreateNpcStateSnapshots()
    .Single(snapshot => snapshot.Replication.ReplicationId == 3);
  if (!restoredTown.HasHome || restoredTown.Home.HomeTileX != 8 ||
      restoredTown.Faction != NpcFaction.Town || restoredTown.Category != NpcCategory.Town ||
      !restoredSegment.HasSegment || restoredSegment.Segment.Root != new NpcHandle(3) ||
      restoredSegment.Category != NpcCategory.Segment)
  {
    throw new InvalidOperationException("NPC composition state did not survive Dome snapshot restore.");
  }

  NpcStateProjector projector = new();
  NpcProjectionResult projection = projector.Project(state);
  if (!projection.IsSupported || projection.Packet.Identity != npc.Value ||
      projection.Packet.NpcType != state.NetId)
  {
    throw new InvalidOperationException("Supported NPC behavior did not project to SyncNPC DTO.");
  }

  byte[] encoded = NpcSyncPacketCodec.Encode(projection.Packet);
  NpcSyncPacket decoded = NpcSyncPacketCodec.Decode(encoded);
  if (decoded.Identity != projection.Packet.Identity || decoded.NpcType != projection.Packet.NpcType ||
      decoded.Target != projection.Packet.Target || decoded.DirectionRight != projection.Packet.DirectionRight)
  {
    throw new InvalidOperationException("SyncNPC codec did not preserve identity and sparse state.");
  }

  NpcStateSnapshot floatingEyeState = state with
  {
    Replication = state.Replication with { ReplicationId = 4 },
    Behavior = new NpcBehaviorStateComponent(
      NpcBehaviorId.FloatingEye,
      state.Behavior.Chase,
      state.Behavior.TownHome,
      new NpcFlyingState(0.75f, 0.5f, 6.0f, 4.0f))
  };
  NpcStateSnapshot floatingEyeReplicationRestore = NpcStateSnapshot.FromReplication(
    floatingEyeState.ToReplicationSnapshot());
  if (floatingEyeReplicationRestore.Behavior.Flying != floatingEyeState.Behavior.Flying)
  {
    throw new InvalidOperationException(
      "FloatingEye typed movement state was lost during replication snapshot restore.");
  }

  NpcStateSnapshot townReplicationRestore = NpcStateSnapshot.FromReplication(
    townState.ToReplicationSnapshot());
  if (townReplicationRestore.Faction != NpcFaction.Town ||
      townReplicationRestore.Category != NpcCategory.Town)
  {
    throw new InvalidOperationException(
      "NPC faction/category state was lost during replication snapshot restore.");
  }
  NpcProjectionResult floatingEyeProjection = projector.Project(floatingEyeState);
  if (!floatingEyeProjection.IsSupported || floatingEyeProjection.Packet.Ai0 != 0.75f ||
      floatingEyeProjection.Packet.Ai1 != 0.5f || floatingEyeProjection.Packet.Ai2 != 6.0f ||
      floatingEyeProjection.Packet.Ai3 != 4.0f)
  {
    throw new InvalidOperationException("FloatingEye movement state did not project to sparse SyncNPC AI.");
  }

  NpcSyncPacket decodedFloatingEye = NpcSyncPacketCodec.Decode(
    NpcSyncPacketCodec.Encode(floatingEyeProjection.Packet));
  if (decodedFloatingEye.Ai0 != 0.75f || decodedFloatingEye.Ai1 != 0.5f ||
      decodedFloatingEye.Ai2 != 6.0f || decodedFloatingEye.Ai3 != 4.0f)
  {
    throw new InvalidOperationException("FloatingEye sparse AI values did not survive SyncNPC codec round-trip.");
  }

  NpcStateSnapshot invalidFloatingEyeState = floatingEyeState with
  {
    Behavior = new NpcBehaviorStateComponent(
      NpcBehaviorId.FloatingEye,
      state.Behavior.Chase,
      state.Behavior.TownHome,
      new NpcFlyingState(float.NaN, 0.5f, 6.0f, 4.0f))
  };
  NpcProjectionResult invalidFloatingEyeProjection = projector.Project(invalidFloatingEyeState);
  if (invalidFloatingEyeProjection.IsSupported)
  {
    throw new InvalidOperationException("Invalid FloatingEye movement state was projected to SyncNPC.");
  }

  NpcSyncPacket lifeWidthPacket = projection.Packet with
  {
    Life = 75,
    LifeMaximum = 100,
    Ai0 = 1.5f,
    Ai1 = null,
    Revision = 7
  };
  byte[] lifeWidthBytes = NpcSyncPacketCodec.Encode(lifeWidthPacket);
  byte[] expectedLifeWidthBytes =
  [
    33, 0, 23,
    1, 0,
    0, 0, 160, 67,
    0, 0, 0, 0,
    0, 0, 0, 0,
    0, 0, 0, 0,
    0, 0,
    4,
    0,
    0, 0, 192, 63,
    1, 0,
    1,
    75
  ];
  if (!lifeWidthBytes.SequenceEqual(expectedLifeWidthBytes))
  {
    throw new InvalidOperationException("SyncNPC update byte fixture diverged.");
  }

  NpcSyncPacket decodedLifeWidth = NpcSyncPacketCodec.Decode(lifeWidthBytes);
  if (decodedLifeWidth.Life != 75 || decodedLifeWidth.Ai0 != 1.5f || decodedLifeWidth.Ai1 is not null)
  {
    throw new InvalidOperationException("SyncNPC sparse AI or short life-width fixture failed.");
  }

  NpcSyncPacket wideLifePacket = lifeWidthPacket with { Life = 40000, LifeMaximum = 50000 };
  NpcSyncPacket decodedWideLife = NpcSyncPacketCodec.Decode(NpcSyncPacketCodec.Encode(wideLifePacket));
  if (decodedWideLife.Life != 40000)
  {
    throw new InvalidOperationException("SyncNPC int life-width fixture failed.");
  }

  NpcSyncPacket inactivePacket = lifeWidthPacket with { Life = 0, Revision = 8 };
  NpcSyncPacket decodedInactive = NpcSyncPacketCodec.Decode(NpcSyncPacketCodec.Encode(inactivePacket));
  if (decodedInactive.Identity != lifeWidthPacket.Identity || decodedInactive.Life != 0 ||
      inactivePacket.Revision <= lifeWidthPacket.Revision)
  {
    throw new InvalidOperationException("SyncNPC inactive fixture did not preserve stable identity.");
  }

  NpcStateSnapshot unsupported = state with
  {
    Behavior = new NpcBehaviorStateComponent(
      (NpcBehaviorId)99,
      state.Behavior.Chase,
      state.Behavior.TownHome)
  };
  NpcProjectionResult unsupportedProjection = projector.Project(unsupported);
  if (unsupportedProjection.IsSupported || string.IsNullOrWhiteSpace(unsupportedProjection.UnsupportedReason))
  {
    throw new InvalidOperationException("Unsupported NPC behavior was guessed instead of reported.");
  }

  bool rejectedUndefinedReplicationState = false;
  try
  {
    _ = NpcStateSnapshot.FromReplication(
      state.ToReplicationSnapshot() with { Faction = (NpcFaction)99 });
  }
  catch (ArgumentOutOfRangeException)
  {
    rejectedUndefinedReplicationState = true;
  }

  if (!rejectedUndefinedReplicationState)
  {
    throw new InvalidOperationException(
      "NPC replication restore accepted an undefined faction value.");
  }
}

Console.WriteLine("PASS: NPC state snapshot round-trip and SyncNPC projection/codec fixtures");
