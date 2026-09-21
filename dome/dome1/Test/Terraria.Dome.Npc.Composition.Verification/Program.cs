using System;
using System.Collections.Generic;
using System.Linq;
using Arch.Core;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Movement.Components;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.Npc.Systems;

using ArchWorld = Arch.Core.World;

using (ArchWorld world = ArchWorld.Create())
{
  NpcHandle townNpc = new(1);
  Entity townEntity = world.Create(
    new NpcTagComponent(),
    new NpcHomeComponent(12, 8, isHomeless: false, returnTimeoutTicks: 4, townVariant: 2),
    new MovementIntentComponent());
  ref NpcHomeComponent home = ref world.Get<NpcHomeComponent>(townEntity);
  ref MovementIntentComponent intent = ref world.Get<MovementIntentComponent>(townEntity);
  NpcHomeResult homeResult = new NpcHomeSystem().Advance(
    ref home,
    ref intent,
    new SimulationVector(8.0f, 8.0f),
    isDayTime: true);
  if (homeResult.HorizontalVelocity <= 0.0f || homeResult.Facing != 1 ||
      !intent.HasNpcIntent || home.IsHomeless)
  {
    throw new InvalidOperationException("Town NPC home behavior did not produce a return intent.");
  }

  NpcInteractionSystem interactionSystem = new();
  if (!interactionSystem.CanTalk(true, 7, 0.0f, isTownPet: false) ||
      interactionSystem.CanTalk(true, 7, 0.1f, isTownPet: false) ||
      interactionSystem.CanTalk(true, 7, 0.0f, isTownPet: true) ||
      interactionSystem.CanBeTalkedTo(false, 7, 0.0f) ||
      interactionSystem.CanBeTalkedTo(true, 6, 0.0f) ||
      interactionSystem.CanBeTalkedTo(true, 7, float.NaN))
  {
    throw new InvalidOperationException("NPC talk eligibility did not preserve the legacy boundary.");
  }

  NpcDefinition talkableTownDefinition = new(
    DefinitionId: 20,
    NetId: 20,
    MaximumHealth: 100,
    Defense: 0,
    ColliderWidth: 1.0f,
    ColliderHeight: 2.0f,
    BehaviorId: NpcBehaviorId.TownHome,
    LootTableId: 1,
    Faction: NpcFaction.Town,
    Category: NpcCategory.Town,
    AiStyle: 7,
    IsLikeTownNpc: true);
  if (!interactionSystem.CanTalk(talkableTownDefinition, 0.0f) ||
      !interactionSystem.CanBeTalkedTo(talkableTownDefinition, 0.0f))
  {
    throw new InvalidOperationException("NPC definition talk capabilities did not reach the pure query.");
  }

  NpcDefinition townPetDefinition = new(
    DefinitionId: 20,
    NetId: 20,
    MaximumHealth: 100,
    Defense: 0,
    ColliderWidth: 1.0f,
    ColliderHeight: 2.0f,
    BehaviorId: NpcBehaviorId.TownHome,
    LootTableId: 1,
    Faction: NpcFaction.Town,
    Category: NpcCategory.Town,
    AiStyle: 7,
    IsLikeTownNpc: true,
    IsTownPet: true);
  if (interactionSystem.CanTalk(townPetDefinition, 0.0f) ||
      !interactionSystem.CanBeTalkedTo(townPetDefinition, 0.0f))
  {
    throw new InvalidOperationException(
      "NPC definition query collapsed the legacy town-pet CanTalk distinction.");
  }

  NpcDefinition nonTownDefinition = new(
    DefinitionId: 21,
    NetId: 21,
    MaximumHealth: 100,
    Defense: 0,
    ColliderWidth: 1.0f,
    ColliderHeight: 2.0f,
    BehaviorId: NpcBehaviorId.OrdinaryChase,
    LootTableId: 1);
  if (interactionSystem.CanBeTalkedTo(nonTownDefinition, 0.0f))
  {
    throw new InvalidOperationException("NPC definition query inferred town eligibility from defaults.");
  }

  NpcDefinition targetCapableDefinition = new(
    DefinitionId: 22,
    NetId: 22,
    MaximumHealth: 100,
    Defense: 0,
    ColliderWidth: 1.0f,
    ColliderHeight: 2.0f,
    BehaviorId: NpcBehaviorId.OrdinaryChase,
    LootTableId: 1,
    SupportsNpcTargets: true);
  if (!targetCapableDefinition.SupportsNpcTargets || nonTownDefinition.SupportsNpcTargets)
  {
    throw new InvalidOperationException(
      "NPC target capability did not remain an explicit definition input.");
  }

  NpcDefinition bossDefinition = new(
    DefinitionId: 23,
    NetId: 23,
    MaximumHealth: 100,
    Defense: 0,
    ColliderWidth: 1.0f,
    ColliderHeight: 2.0f,
    BehaviorId: NpcBehaviorId.OrdinaryChase,
    LootTableId: 1,
    IsBoss: true);
  NpcDefinition rainbowBoulderDefinition = new(
    DefinitionId: 24,
    NetId: 24,
    MaximumHealth: 100,
    Defense: 0,
    ColliderWidth: 1.0f,
    ColliderHeight: 2.0f,
    BehaviorId: NpcBehaviorId.OrdinaryChase,
    LootTableId: 1,
    ShouldBeCountedAsBossForRainbowBoulders: true);
  if (!bossDefinition.TreatedAsABossForRainbowBoulders ||
      !rainbowBoulderDefinition.TreatedAsABossForRainbowBoulders ||
      nonTownDefinition.TreatedAsABossForRainbowBoulders)
  {
    throw new InvalidOperationException(
      "NPC boss classification did not preserve explicit definition capabilities.");
  }

  NpcGivenNameComponent givenName = new(null);
  if (givenName.HasGivenName || givenName.GivenName != string.Empty)
  {
    throw new InvalidOperationException("NPC given-name state did not normalize null to empty.");
  }

  givenName.SetGivenName("Guide");
  if (!givenName.HasGivenName || givenName.GivenName != "Guide")
  {
    throw new InvalidOperationException("NPC given-name state did not preserve a non-empty name.");
  }

  givenName.SetGivenName(null);
  if (givenName.HasGivenName || givenName.GivenName != string.Empty)
  {
    throw new InvalidOperationException("NPC given-name state did not clear through null normalization.");
  }

  if (!interactionSystem.TryCreate(
        new PlayerHandle(2),
        townNpc,
        NpcInteractionKind.Talk,
        "session-2",
        new SimulationVector(10.0f, 8.0f),
        new SimulationVector(12.0f, 8.0f),
        maximumRange: 3.0f,
        playerIsActive: true,
        npcIsActive: true,
        out NpcInteractionCommand interaction,
        faction: NpcFaction.Town) ||
      interaction.Player.Value != 2 || interaction.Npc != townNpc ||
      interaction.Kind != NpcInteractionKind.Talk)
  {
    throw new InvalidOperationException("NPC interaction did not preserve the verified session owner.");
  }

  NpcInteractionComponent interactionState = new();
  interactionSystem.Apply(ref interactionState, interaction, tick: 12);
  if (!interactionState.HasActiveInteraction || interactionState.Player.Value != 2 ||
      interactionState.Kind != NpcInteractionKind.Talk || interactionState.SessionId != "session-2" ||
      interactionState.LastInteractionTick != 12)
  {
    throw new InvalidOperationException("NPC interaction state did not apply the authorized command.");
  }

  if (interactionSystem.TryExpire(ref interactionState, currentTick: 15, timeoutTicks: 4) ||
      !interactionState.HasActiveInteraction ||
      !interactionSystem.TryExpire(ref interactionState, currentTick: 17, timeoutTicks: 4) ||
      interactionState.HasActiveInteraction)
  {
    throw new InvalidOperationException("NPC interaction state did not expire deterministically.");
  }

  if (interactionSystem.TryCreate(
        new PlayerHandle(2),
        townNpc,
        NpcInteractionKind.Shop,
        "session-2",
        new SimulationVector(30.0f, 8.0f),
        new SimulationVector(12.0f, 8.0f),
        maximumRange: 3.0f,
        playerIsActive: true,
        npcIsActive: true,
        out _,
        faction: NpcFaction.Town) ||
      interactionSystem.TryCreate(
        new PlayerHandle(2),
        townNpc,
        NpcInteractionKind.Talk,
        "",
        new SimulationVector(10.0f, 8.0f),
        new SimulationVector(12.0f, 8.0f),
        maximumRange: 3.0f,
        playerIsActive: true,
        npcIsActive: true,
        out _,
        faction: NpcFaction.Town))
  {
    throw new InvalidOperationException("NPC interaction accepted an unauthorized session or range.");
  }

  if (interactionSystem.TryCreate(
        new PlayerHandle(2),
        townNpc,
        NpcInteractionKind.Talk,
        "session-2",
        new SimulationVector(10.0f, 8.0f),
        new SimulationVector(12.0f, 8.0f),
        maximumRange: 3.0f,
        playerIsActive: true,
        npcIsActive: true,
        out _,
        faction: NpcFaction.Hostile))
  {
    throw new InvalidOperationException("NPC interaction accepted a hostile NPC faction.");
  }

  home.ReturnTimeoutTicks = -1;
  try
  {
    _ = new NpcHomeSystem().Advance(
      ref home,
      ref intent,
      new SimulationVector(12.0f, 8.0f),
      isDayTime: true);
    throw new InvalidOperationException("NPC home behavior accepted a negative timeout.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  home.ReturnTimeoutTicks = 4;

  NpcHomeSnapshotValue townSnapshot = new(
    townNpc,
    home.HomeTileX,
    home.HomeTileY,
    home.IsHomeless,
    home.ReturnTimeoutTicks,
    home.TownVariant);
  using (ArchWorld restoredWorld = ArchWorld.Create())
  {
    Entity restoredTown = restoredWorld.Create(
      new NpcTagComponent(),
      new NpcHomeComponent(
        townSnapshot.HomeTileX,
        townSnapshot.HomeTileY,
        townSnapshot.IsHomeless,
        townSnapshot.ReturnTimeoutTicks,
        townSnapshot.TownVariant));
    NpcHomeComponent restoredHome = restoredWorld.Get<NpcHomeComponent>(restoredTown);
    if (restoredHome.HomeTileX != home.HomeTileX ||
        restoredHome.HomeTileY != home.HomeTileY ||
        restoredHome.IsHomeless != home.IsHomeless ||
        restoredHome.ReturnTimeoutTicks != home.ReturnTimeoutTicks ||
        restoredHome.TownVariant != home.TownVariant)
    {
      throw new InvalidOperationException("Town NPC home state did not survive snapshot restore.");
    }
  }

  NpcHandle rootNpc = new(10);
  NpcHandle childNpc = new(11);
  NpcSegmentComponent rootSegment = new(
    rootNpc,
    default,
    childNpc,
    segmentIndex: 0,
    isRoot: true,
    NpcSegmentLifePolicy.RootShared);
  NpcSegmentComponent childSegment = new(
    rootNpc,
    rootNpc,
    default,
    segmentIndex: 1,
    isRoot: false,
    NpcSegmentLifePolicy.RootShared);
  NpcSegmentState[] segmentStates =
  [
    new(childNpc, childSegment, IsActive: true, IsDead: false),
    new(rootNpc, rootSegment, IsActive: false, IsDead: true)
  ];
  NpcSegmentLifecycleSystem segmentSystem = new();
  NpcSegmentValidationResult validation = segmentSystem.Validate(segmentStates);
  if (!validation.IsValid)
  {
    throw new InvalidOperationException(validation.FailureReason);
  }

  NpcSegmentState invalidLifePolicy = segmentStates[0] with
  {
    Segment = childSegment with { LifePolicy = (NpcSegmentLifePolicy)99 }
  };
  if (segmentSystem.Validate([invalidLifePolicy, segmentStates[1]]).IsValid)
  {
    throw new InvalidOperationException("Segment validation accepted an undefined life policy.");
  }

  NpcHandle[] deathOrder = segmentSystem.GetDeathOrder(segmentStates).ToArray();
  if (!deathOrder.SequenceEqual([childNpc, rootNpc]))
  {
    throw new InvalidOperationException("Segment death order did not resolve child before root.");
  }

  IReadOnlyList<DespawnNpcCommand> wormDespawns = segmentSystem.GetWormFollowUpDespawns(
    segmentStates,
    rootNpc,
    new HashSet<NpcHandle> { childNpc });
  if (wormDespawns.Count != 1 || wormDespawns[0].Npc != childNpc ||
      wormDespawns[0].Reason != NpcDespawnReason.Killed)
  {
    throw new InvalidOperationException("Worm follow-up despawn did not stop at the typed child chain.");
  }

  NpcSegmentState[] livingSegmentStates =
  [
    new(childNpc, childSegment, IsActive: true, IsDead: false),
    new(rootNpc, rootSegment, IsActive: true, IsDead: false)
  ];
  if (segmentSystem.GetWormFollowUpDespawns(
        livingSegmentStates,
        rootNpc,
        new HashSet<NpcHandle> { childNpc }).Count != 0)
  {
    throw new InvalidOperationException("An active, living trigger incorrectly despawned worm children.");
  }

  if (segmentSystem.GetWormFollowUpDespawns(
        segmentStates,
        rootNpc,
        new HashSet<NpcHandle>()).Count != 0)
  {
    throw new InvalidOperationException("Non-worm segments were incorrectly despawned.");
  }

  NpcSegmentState duplicateIndex = segmentStates[0] with
  {
    Segment = childSegment with { Root = rootNpc, SegmentIndex = 0 }
  };
  if (segmentSystem.Validate([duplicateIndex, segmentStates[1]]).IsValid)
  {
    throw new InvalidOperationException(
      "Segment validation accepted duplicate indexes within one root relationship.");
  }

  SegmentSnapshotValue[] segmentSnapshots = segmentStates
    .Select(state => new SegmentSnapshotValue(
      state.Handle,
      state.Segment.Root,
      state.Segment.Parent,
      state.Segment.Child,
      state.Segment.SegmentIndex,
      state.Segment.IsRoot,
      state.Segment.LifePolicy,
      state.IsActive,
      state.IsDead))
    .ToArray();
  using (ArchWorld restoredSegments = ArchWorld.Create())
  {
    for (int index = 0; index < segmentSnapshots.Length; index++)
    {
      SegmentSnapshotValue snapshot = segmentSnapshots[index];
      Entity entity = restoredSegments.Create(
        new NpcTagComponent(),
        new NpcSegmentComponent(
          snapshot.Root,
          snapshot.Parent,
          snapshot.Child,
          snapshot.SegmentIndex,
          snapshot.IsRoot,
          snapshot.LifePolicy));
      NpcSegmentComponent restored = restoredSegments.Get<NpcSegmentComponent>(entity);
      if (restored.Root != snapshot.Root || restored.Parent != snapshot.Parent ||
          restored.Child != snapshot.Child || restored.SegmentIndex != snapshot.SegmentIndex ||
          restored.IsRoot != snapshot.IsRoot || restored.LifePolicy != snapshot.LifePolicy)
      {
        throw new InvalidOperationException("Segment relationship did not survive snapshot restore.");
      }
    }
  }
}

if (typeof(NpcHomeComponent).IsClass || typeof(NpcSegmentComponent).IsClass)
{
  throw new InvalidOperationException("NPC composition must use components, not NPC subclasses.");
}

Console.WriteLine("PASS: NPC town-home and segment composition snapshot/restore");

internal readonly record struct NpcHomeSnapshotValue(
  NpcHandle Npc,
  short HomeTileX,
  short HomeTileY,
  bool IsHomeless,
  int ReturnTimeoutTicks,
  int TownVariant);

internal readonly record struct SegmentSnapshotValue(
  NpcHandle Handle,
  NpcHandle Root,
  NpcHandle Parent,
  NpcHandle Child,
  int SegmentIndex,
  bool IsRoot,
  NpcSegmentLifePolicy LifePolicy,
  bool IsActive,
  bool IsDead);
