using System;
using System.Linq;
using Arch.Core;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Movement.Components;
using Terraria.Dome.Simulation.Npc.Components;
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
    new(rootNpc, rootSegment, IsActive: true, IsDead: false)
  ];
  NpcSegmentLifecycleSystem segmentSystem = new();
  NpcSegmentValidationResult validation = segmentSystem.Validate(segmentStates);
  if (!validation.IsValid)
  {
    throw new InvalidOperationException(validation.FailureReason);
  }

  NpcHandle[] deathOrder = segmentSystem.GetDeathOrder(segmentStates).ToArray();
  if (!deathOrder.SequenceEqual([childNpc, rootNpc]))
  {
    throw new InvalidOperationException("Segment death order did not resolve child before root.");
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
