using EntityEcs;
using Terraria.Relationships;
using Terraria.WorldInteraction.TileEntities;
using Terraria.WorldInteraction.Tiles;
using Terraria.WorldStorage;

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static void RequireThrows<TException>(Action action, string message)
  where TException : Exception
{
  try
  {
    action();
  }
  catch (TException)
  {
    return;
  }

  throw new InvalidOperationException(message);
}

static TileEntitySnapshot TrainingDummy(int id, TileCoordinate anchor, short npcIndex = -1)
{
  return new TileEntitySnapshot(
    new TileEntityId(id),
    new TileEntityTypeId(0),
    anchor,
    Array.Empty<Terraria.Items.ItemState>(),
    npcIndex);
}

static TileEntitySnapshot LogicSensor(
  int id,
  TileCoordinate anchor,
  byte logicCheck,
  bool isOn = false)
{
  return new TileEntitySnapshot(
    new TileEntityId(id),
    new TileEntityTypeId(2),
    anchor,
    Array.Empty<Terraria.Items.ItemState>(),
    logicCheck: logicCheck,
    logicOn: isOn);
}

static void VerifyRuntimeRootIndexesAndTypedCapture()
{
  using EntityRuntime runtime = new();
  TileEntityUpdateSchedule updates = new();
  TileEntityStore store = new(runtime, updates);
  TileCoordinate dummyAnchor = new(17, 23);
  TileCoordinate sensorAnchor = new(29, 31);
  TileEntitySnapshot dummyDto = TrainingDummy(0, dummyAnchor);
  TileEntitySnapshot sensorDto = LogicSensor(1, sensorAnchor, logicCheck: 0);

  TileEntityRestoreSystem.Apply(store, new[] { dummyDto, sensorDto }, nextId: 2);

  EntityReference dummyById = RequireReference(store, new TileEntityId(0));
  EntityReference dummyByAnchor = RequireAnchorReference(store, dummyAnchor);
  EntityReference sensorById = RequireReference(store, new TileEntityId(1));
  EntityReference sensorByAnchor = RequireAnchorReference(store, sensorAnchor);
  Require(dummyById == dummyByAnchor,
    "The ID and anchor indexes must resolve to the same Training Dummy root.");
  Require(sensorById == sensorByAnchor,
    "The ID and anchor indexes must resolve to the same Logic Sensor root.");
  Require(runtime.TryResolve(dummyById, out RuntimeEntityHandle dummyHandle),
    "The indexed Training Dummy root must resolve in the shared runtime.");
  Require(runtime.TryResolve(sensorById, out RuntimeEntityHandle sensorHandle),
    "The indexed Logic Sensor root must resolve in the shared runtime.");
  Require(runtime.Has<TileEntityBindingComponent>(dummyHandle),
    "A TileEntity root must have the ID/type/anchor binding capability.");
  Require(runtime.Has<TileEntityTrainingDummyComponent>(dummyHandle),
    "A Training Dummy root must own its runtime capability component.");
  Require(!runtime.Has<TileEntityLogicSensorComponent>(dummyHandle),
    "A Training Dummy root must not receive Logic Sensor state.");
  Require(runtime.Has<TileEntityLogicSensorComponent>(sensorHandle),
    "A Logic Sensor root must own its runtime capability component.");
  Require(!runtime.Has<TileEntityTrainingDummyComponent>(sensorHandle),
    "A Logic Sensor root must not receive Training Dummy state.");
  Require(!runtime.Has<TileEntitySnapshot>(dummyHandle),
    "Persistence DTOs must not be attached as runtime components.");

  TileEntitySnapshot capturedDummyBeforeWrite = RequireSnapshot(store, new TileEntityId(0));
  Require(store.CommitTrainingDummyNpcIndex(new TileEntityId(0), npcIndex: 12),
    "The Training Dummy capability must accept its NPC slot projection.");
  TileEntitySnapshot capturedDummyAfterWrite = RequireSnapshot(store, new TileEntityId(0));
  Require(capturedDummyBeforeWrite.NpcIndex == -1 && capturedDummyAfterWrite.NpcIndex == 12,
    "Snapshot capture must project live Training Dummy state without changing old DTOs.");

  TileEntitySnapshot capturedSensorBeforeWrite = RequireSnapshot(store, new TileEntityId(1));
  Require(store.CommitLogicSensorState(new TileEntityId(1), isOn: true, countedData: 5),
    "A Logic Sensor state transition must report its changed output state.");
  TileEntitySnapshot capturedSensorAfterWrite = RequireSnapshot(store, new TileEntityId(1));
  Require(!capturedSensorBeforeWrite.LogicOn && capturedSensorAfterWrite.LogicOn,
    "Snapshot capture must read Logic Sensor state from its runtime capability component.");
  Require(store.TryGetLogicSensorCountedData(new TileEntityId(1), out int countedData)
      && countedData == 5,
    "The transient Logic Sensor countdown must live in its runtime component.");
  Require(updates.IsScheduled(new TileEntityId(0)) && updates.IsScheduled(new TileEntityId(1)),
    "The shared schedule must include both currently supported update capabilities.");
}

static void VerifyAllCurrentLogicChecksSurviveHydrationAndCapture()
{
  using EntityRuntime runtime = new();
  TileEntityUpdateSchedule updates = new();
  TileEntityStore store = new(runtime, updates);
  var snapshots = new List<TileEntitySnapshot>();
  for (byte logicCheck = 0; logicCheck <= 7; logicCheck++)
  {
    snapshots.Add(LogicSensor(logicCheck, new TileCoordinate(10 + logicCheck, 40), logicCheck));
  }

  TileEntityRestoreSystem.Apply(store, snapshots, nextId: snapshots.Count);
  IReadOnlyList<TileEntitySnapshot> captured = store.CreateSnapshot();
  Require(captured.Count == 8, "All eight currently supported Logic Sensor checks must hydrate.");
  for (byte logicCheck = 0; logicCheck <= 7; logicCheck++)
  {
    Require(captured[logicCheck].LogicCheck == logicCheck,
      "Capture must preserve the Logic Sensor check value from its runtime component.");
  }
}

static void VerifyLiquidSensorActivationAndReleaseDelay()
{
  const byte waterCheck = (byte)LogicCheckType.Water;
  const byte lavaCheck = (byte)LogicCheckType.Lava;
  const byte honeyCheck = (byte)LogicCheckType.Honey;
  const byte anyLiquidCheck = (byte)LogicCheckType.Liquid;
  const byte waterType = (byte)LiquidKind.Water;
  const byte lavaType = (byte)LiquidKind.Lava;
  const byte honeyType = (byte)LiquidKind.Honey;
  const byte shimmerType = (byte)LiquidKind.Shimmer;

  (byte LogicCheck, byte LiquidType, bool Expected)[] activationCases =
  {
    (waterCheck, waterType, true),
    (waterCheck, lavaType, false),
    (waterCheck, honeyType, false),
    (waterCheck, shimmerType, false),
    (lavaCheck, waterType, false),
    (lavaCheck, lavaType, true),
    (lavaCheck, honeyType, false),
    (lavaCheck, shimmerType, false),
    (honeyCheck, waterType, false),
    (honeyCheck, lavaType, false),
    (honeyCheck, honeyType, true),
    (honeyCheck, shimmerType, false),
    (anyLiquidCheck, waterType, true),
    (anyLiquidCheck, lavaType, true),
    (anyLiquidCheck, honeyType, true),
    (anyLiquidCheck, shimmerType, true),
  };

  foreach ((byte logicCheck, byte liquidType, bool expected) in activationCases)
  {
    var tile = new TileCellState { LiquidAmount = byte.MaxValue, LiquidType = liquidType };
    bool isOn = TileEntityLiquidSensorSystem.Evaluate(
      logicCheck,
      tile,
      isOn: false,
      currentCountedData: 6,
      out int countedData);
    Require(isOn == expected,
      $"Logic check {logicCheck} must evaluate liquid type {liquidType} as {expected}.");
    Require(countedData == 6,
      "An off Logic Sensor evaluation must leave CountedData unchanged.");
  }

  foreach (byte logicCheck in new[] { waterCheck, lavaCheck, honeyCheck, anyLiquidCheck })
  {
    var emptyTile = new TileCellState { LiquidAmount = 0, LiquidType = waterType };
    bool emptyState = TileEntityLiquidSensorSystem.Evaluate(
      logicCheck,
      emptyTile,
      isOn: false,
      currentCountedData: 0,
      out int emptyCountedData);
    Require(!emptyState && emptyCountedData == 0,
      $"Logic check {logicCheck} must remain off with no liquid.");
  }

  var waterTile = new TileCellState { LiquidAmount = 1, LiquidType = waterType };
  bool activated = TileEntityLiquidSensorSystem.Evaluate(
    waterCheck,
    waterTile,
    isOn: false,
    currentCountedData: 0,
    out int activationCountedData);
  Require(activated && activationCountedData == 0,
    "A matching liquid must activate the sensor without starting the release countdown.");

  var empty = new TileCellState { LiquidAmount = 0, LiquidType = waterType };
  bool remainsOn = TileEntityLiquidSensorSystem.Evaluate(
    waterCheck,
    empty,
    isOn: true,
    currentCountedData: activationCountedData,
    out int remainingTicks);
  Require(remainsOn && remainingTicks == 15,
    "The first liquid-free update must start the 15-tick release countdown.");

  for (int expectedTicks = 14; expectedTicks > 8; expectedTicks--)
  {
    remainsOn = TileEntityLiquidSensorSystem.Evaluate(
      waterCheck,
      empty,
      remainsOn,
      remainingTicks,
      out int nextRemainingTicks);
    remainingTicks = nextRemainingTicks;
    Require(remainsOn && remainingTicks == expectedTicks,
      "The liquid sensor must stay on while its release countdown is positive.");
  }

  bool reactivated = TileEntityLiquidSensorSystem.Evaluate(
    waterCheck,
    waterTile,
    remainsOn,
    remainingTicks,
    out int reactivatedCountedData);
  Require(reactivated && reactivatedCountedData == remainingTicks,
    "Matching liquid must pause without resetting the partially elapsed release countdown.");
  remainsOn = TileEntityLiquidSensorSystem.Evaluate(
    waterCheck,
    empty,
    reactivated,
    reactivatedCountedData,
    out int resumedRemainingTicks);
  Require(remainsOn && resumedRemainingTicks == remainingTicks - 1,
    "A later liquid loss must resume the stored release countdown.");
  remainingTicks = resumedRemainingTicks;

  for (int expectedTicks = remainingTicks - 1; expectedTicks > 0; expectedTicks--)
  {
    remainsOn = TileEntityLiquidSensorSystem.Evaluate(
      waterCheck,
      empty,
      remainsOn,
      remainingTicks,
      out int nextRemainingTicks);
    remainingTicks = nextRemainingTicks;
    Require(remainsOn && remainingTicks == expectedTicks,
      "The resumed release countdown must stay on while it is positive.");
  }

  bool released = TileEntityLiquidSensorSystem.Evaluate(
    waterCheck,
    empty,
    remainsOn,
    remainingTicks,
    out int releasedCountedData);
  Require(!released && releasedCountedData == 0,
    "The liquid sensor must turn off when the 15-tick release countdown expires.");
}

static void VerifyBorrowedRemovalAndReplaceFailureAreAtomic()
{
  using EntityRuntime runtime = new();
  TileEntityUpdateSchedule updates = new();
  TileEntityStore store = new(runtime, updates);
  TileCoordinate dummyAnchor = new(11, 13);
  TileCoordinate sensorAnchor = new(19, 23);
  TileEntityRestoreSystem.Apply(
    store,
    new[]
    {
      TrainingDummy(0, dummyAnchor, npcIndex: 7),
      LogicSensor(1, sensorAnchor, 1, isOn: true)
    },
    nextId: 2);

  EntityReference dummyReference = RequireReference(store, new TileEntityId(0));
  EntityReference sensorReference = RequireReference(store, new TileEntityId(1));
  Require(runtime.TryResolve(dummyReference, out RuntimeEntityHandle dummyHandle),
    "The borrowed-remove scenario requires a live Training Dummy root.");
  int originalEntityCount = runtime.EntityCount;
  updates.CaptureTickSnapshot();
  updates.CompleteTickSnapshot();

  bool removalRejectedWhileBorrowed = false;
  Require(runtime.TryInspect(
      dummyHandle,
      (in TileEntityBindingComponent _) =>
      {
        removalRejectedWhileBorrowed = !store.Remove(new TileEntityId(0));
      }),
    "The store must be able to inspect the borrowed root during the failure probe.");
  Require(removalRejectedWhileBorrowed,
    "A borrowed runtime entity must reject removal without entering termination.");
  Require(runtime.EntityCount == originalEntityCount && updates.IsScheduled(new TileEntityId(0)),
    "Borrowed removal rejection must preserve root count and the shared schedule.");
  Require(RequireReference(store, new TileEntityId(0)) == dummyReference &&
      RequireAnchorReference(store, dummyAnchor) == dummyReference,
    "Borrowed removal rejection must preserve both indexes and the same root identity.");

  var lateCandidate = TrainingDummy(2, new TileCoordinate(27, 29));
  TileEntitySnapshot[] failedReplacement = new[] { lateCandidate }
    .Concat(store.CreateSnapshot())
    .ToArray();
  bool replacementRejectedWhileBorrowed = false;
  Require(runtime.TryInspect(
      dummyHandle,
      (in TileEntityBindingComponent _) =>
      {
        try
        {
          store.CommitRuntimeSnapshot(new TileEntityStoreSnapshot(failedReplacement, nextId: 3));
        }
        catch (InvalidOperationException)
        {
          replacementRejectedWhileBorrowed = true;
        }
      }),
    "The store must be able to inspect the root while replacement is rejected.");
  Require(replacementRejectedWhileBorrowed,
    "Replacement must reject an existing same-ID root whose runtime state is borrowed.");
  Require(runtime.EntityCount == originalEntityCount && store.Count == 2,
    "Failed replacement must remove every prepared candidate root.");
  Require(RequireReference(store, new TileEntityId(0)) == dummyReference &&
      RequireReference(store, new TileEntityId(1)) == sensorReference &&
      RequireAnchorReference(store, dummyAnchor) == dummyReference &&
      RequireAnchorReference(store, sensorAnchor) == sensorReference,
    "Failed replacement must preserve all ID/anchor mappings to their original roots.");
  Require(updates.IsScheduled(new TileEntityId(0)) && updates.IsScheduled(new TileEntityId(1))
      && !updates.IsScheduled(new TileEntityId(2)),
    "Failed replacement must leave the shared schedule unchanged.");

  TileEntitySnapshot[] duplicateAnchors =
  {
    TrainingDummy(0, dummyAnchor),
    LogicSensor(1, dummyAnchor, logicCheck: 1)
  };
  RequireThrows<InvalidDataException>(
    () => TileEntityRestoreSystem.Apply(store, duplicateAnchors, nextId: 2),
    "A duplicate anchor must be rejected before replacing runtime roots.");
  Require(runtime.EntityCount == originalEntityCount && store.Count == 2 &&
      RequireReference(store, new TileEntityId(0)) == dummyReference &&
      RequireReference(store, new TileEntityId(1)) == sensorReference &&
      updates.IsScheduled(new TileEntityId(0)) && updates.IsScheduled(new TileEntityId(1)),
    "Duplicate-anchor rejection must preserve roots, indexes and schedule.");

  TileEntitySnapshot[] duplicateIds =
  {
    TrainingDummy(0, dummyAnchor),
    LogicSensor(0, sensorAnchor, logicCheck: 1)
  };
  RequireThrows<InvalidDataException>(
    () => TileEntityRestoreSystem.Apply(store, duplicateIds, nextId: 2),
    "A duplicate TileEntity ID must be rejected before replacing runtime roots.");
  Require(runtime.EntityCount == originalEntityCount && store.Count == 2 &&
      RequireReference(store, new TileEntityId(0)) == dummyReference &&
      RequireReference(store, new TileEntityId(1)) == sensorReference &&
      RequireAnchorReference(store, dummyAnchor) == dummyReference &&
      RequireAnchorReference(store, sensorAnchor) == sensorReference &&
      updates.IsScheduled(new TileEntityId(0)) && updates.IsScheduled(new TileEntityId(1)),
    "Duplicate-ID rejection must preserve roots, indexes and schedule.");
}

static void VerifyTickVisibilityRemovalAndReload()
{
  using EntityRuntime firstRuntime = new();
  TileEntityUpdateSchedule firstSchedule = new();
  TileEntityStore firstStore = new(firstRuntime, firstSchedule);
  TileCoordinate dummyAnchor = new(33, 35);
  TileCoordinate sensorAnchor = new(41, 43);
  TileEntityRestoreSystem.Apply(
    firstStore,
    new[] { TrainingDummy(0, dummyAnchor), LogicSensor(1, sensorAnchor, logicCheck: 0) },
    nextId: 2);

  EntityReference retainedReference = RequireReference(firstStore, new TileEntityId(1));
  IReadOnlyList<TileEntityId> capturedTickIds = firstSchedule.CaptureTickSnapshot();
  TileEntitySnapshot[] withNewEntity = new[] { TrainingDummy(2, new TileCoordinate(47, 53)) }
    .Concat(firstStore.CreateSnapshot())
    .ToArray();
  firstStore.CommitRuntimeSnapshot(new TileEntityStoreSnapshot(withNewEntity, nextId: 3));
  Require(!capturedTickIds.Contains(new TileEntityId(2)),
    "An entity added during a tick must not appear in the already captured ID list.");
  Require(firstSchedule.IsScheduled(new TileEntityId(2)),
    "The newly committed update capability must be scheduled for the next tick.");
  firstSchedule.CompleteTickSnapshot();
  Require(firstSchedule.CaptureTickSnapshot().Contains(new TileEntityId(2)),
    "A newly committed entity must become visible in the next tick capture.");

  IReadOnlyList<TileEntityId> staleTickIds = firstSchedule.CaptureTickSnapshot();
  EntityReference removedReference = RequireReference(firstStore, new TileEntityId(0));
  Require(firstStore.Remove(new TileEntityId(0)), "The Training Dummy root must be removable.");
  Require(!firstSchedule.IsScheduled(new TileEntityId(0)),
    "Removal must cancel the ID in the shared schedule.");
  Require(!firstRuntime.TryResolve(removedReference, out _),
    "A removed root reference must not resolve.");
  Require(!firstStore.TryGetEntityReference(new TileEntityId(0), out _) &&
      !firstStore.TryGetEntityReferenceByAnchor(dummyAnchor, out _),
    "Removal must clear both ID and anchor indexes.");
  Require(staleTickIds.Contains(new TileEntityId(0)) &&
      !firstStore.TryGetRuntimeState(new TileEntityId(0), out _),
    "A removed ID already captured for this tick must safely fail its later ID resolution.");
  firstSchedule.CompleteTickSnapshot();

  using EntityRuntime secondRuntime = new();
  TileEntityUpdateSchedule secondSchedule = new();
  TileEntityStore secondStore = new(secondRuntime, secondSchedule);
  TileEntityRestoreSystem.Apply(secondStore, firstStore.CreateSnapshot(), firstStore.NextId);
  EntityReference reloadedReference = RequireReference(secondStore, new TileEntityId(1));
  Require(reloadedReference.EntityId != retainedReference.EntityId &&
      reloadedReference.RuntimeId != retainedReference.RuntimeId,
    "Reload must preserve the TileEntity DTO ID while creating a fresh runtime root.");
  Require(!secondRuntime.TryResolve(retainedReference, out _),
    "A reference from the previous world runtime must fail in the new runtime.");
  Require(RequireReference(secondStore, new TileEntityId(1)) ==
      RequireAnchorReference(secondStore, sensorAnchor),
    "Reloaded ID and anchor indexes must resolve to the same new runtime root.");
}

static void VerifyDisposedStoreRejectsReuseWithoutAllocatingRoots()
{
  using EntityRuntime runtime = new();
  using TileEntityUpdateSchedule updates = new();
  TileEntityStore store = new(runtime, updates);
  TileCoordinate dummyAnchor = new(57, 61);
  TileCoordinate sensorAnchor = new(67, 71);
  store.CommitRuntimeSnapshot(
    new TileEntityStoreSnapshot(
      new[]
      {
        TrainingDummy(0, dummyAnchor),
        LogicSensor(1, sensorAnchor, logicCheck: 3, isOn: true),
      },
      nextId: 2));

  EntityReference oldDummyReference = RequireReference(store, new TileEntityId(0));
  EntityReference oldSensorReference = RequireReference(store, new TileEntityId(1));
  int rootCountBeforeDispose = runtime.EntityCount;
  Require(rootCountBeforeDispose == 2 && updates.Count == 2,
    "The disposal probe requires two TileEntity roots and their scheduled IDs.");

  store.Dispose();
  int rootCountAfterDispose = runtime.EntityCount;
  Require(rootCountAfterDispose == 0 && updates.Count == 0,
    "Disposal must remove this store's roots and scheduled IDs before closing it.");
  Require(!runtime.TryResolve(oldDummyReference, out _) &&
      !runtime.TryResolve(oldSensorReference, out _),
    "References from a disposed TileEntity store must no longer resolve.");

  RequireThrows<ObjectDisposedException>(() => _ = store.Count,
    "Count must reject access after TileEntity store disposal.");
  RequireThrows<ObjectDisposedException>(() => _ = store.NextId,
    "NextId must reject access after TileEntity store disposal.");
  RequireThrows<ObjectDisposedException>(() => _ = store.MutationRevision,
    "MutationRevision must reject access after TileEntity store disposal.");
  RequireThrows<ObjectDisposedException>(() => store.CreateScheduledIdSnapshot(),
    "Scheduled-ID capture must reject access after TileEntity store disposal.");
  RequireThrows<ObjectDisposedException>(() => store.CreateSnapshot(),
    "Persistence capture must reject access after TileEntity store disposal.");
  RequireThrows<ObjectDisposedException>(
    () => store.TryGetSnapshot(new TileEntityId(0), out _),
    "Snapshot lookup must reject access after TileEntity store disposal.");
  RequireThrows<ObjectDisposedException>(
    () => store.TryGetEntityReference(new TileEntityId(0), out _),
    "ID reference lookup must reject access after TileEntity store disposal.");
  RequireThrows<ObjectDisposedException>(
    () => store.TryGetEntityReferenceByAnchor(dummyAnchor, out _),
    "Anchor reference lookup must reject access after TileEntity store disposal.");
  RequireThrows<ObjectDisposedException>(
    () => store.TryGetRuntimeState(new TileEntityId(0), out _),
    "Runtime-state lookup must reject access after TileEntity store disposal.");
  RequireThrows<ObjectDisposedException>(
    () => store.TryGetLogicSensorCountedData(new TileEntityId(1), out _),
    "Logic Sensor lookup must reject access after TileEntity store disposal.");
  RequireThrows<ObjectDisposedException>(
    () => store.CommitLogicSensorState(new TileEntityId(1), isOn: false, countedData: 0),
    "Logic Sensor writes must reject access after TileEntity store disposal.");
  RequireThrows<ObjectDisposedException>(
    () => store.CommitTrainingDummyNpcIndex(new TileEntityId(0), npcIndex: -1),
    "Training Dummy writes must reject access after TileEntity store disposal.");
  RequireThrows<ObjectDisposedException>(() => store.Remove(new TileEntityId(0)),
    "Removal must reject access after TileEntity store disposal.");

  var candidate = TrainingDummy(2, new TileCoordinate(79, 83));
  RequireThrows<ObjectDisposedException>(
    () => store.CommitRuntimeSnapshot(new TileEntityStoreSnapshot(new[] { candidate }, nextId: 3)),
    "A captured disposed store must reject snapshot reuse before allocating a new root.");
  store.Dispose();

  Require(runtime.EntityCount == rootCountAfterDispose && updates.Count == 0 &&
      !updates.IsScheduled(new TileEntityId(2)),
    "Rejected disposed-store reuse must not allocate roots or repopulate the schedule.");
  Require(!runtime.TryResolve(oldDummyReference, out _) &&
      !runtime.TryResolve(oldSensorReference, out _),
    "Rejected disposed-store reuse must leave both old references stale.");
}

static EntityReference RequireReference(TileEntityStore store, TileEntityId id)
{
  Require(store.TryGetEntityReference(id, out EntityReference reference),
    $"TileEntity ID {id.Value} must resolve to a runtime reference.");
  return reference;
}

static EntityReference RequireAnchorReference(TileEntityStore store, TileCoordinate anchor)
{
  Require(store.TryGetEntityReferenceByAnchor(anchor, out EntityReference reference),
    $"TileEntity anchor ({anchor.X},{anchor.Y}) must resolve to a runtime reference.");
  return reference;
}

static TileEntitySnapshot RequireSnapshot(TileEntityStore store, TileEntityId id)
{
  if (!store.TryGetSnapshot(id, out TileEntitySnapshot? snapshot) || snapshot is null)
  {
    throw new InvalidOperationException(
      $"TileEntity ID {id.Value} must produce a persistence capture.");
  }

  return snapshot;
}

VerifyRuntimeRootIndexesAndTypedCapture();
VerifyAllCurrentLogicChecksSurviveHydrationAndCapture();
VerifyLiquidSensorActivationAndReleaseDelay();
VerifyBorrowedRemovalAndReplaceFailureAreAtomic();
VerifyTickVisibilityRemovalAndReload();
VerifyDisposedStoreRejectsReuseWithoutAllocatingRoots();
Console.WriteLine(
  "PASS: TileEntity runtime roots, capture, indexes, lifecycle, schedule, reload and disposal");
