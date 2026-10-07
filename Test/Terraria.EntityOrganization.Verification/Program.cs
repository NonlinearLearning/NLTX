using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using EntityEcs;
using Terraria.Relationships;

if (args.Length > 0 && args[0] == "--real-world-rollback")
{
  if (args.Length != 3)
  {
    throw new ArgumentException(
      "Usage: --real-world-rollback <small-world.wld> <medium-world.wld>");
  }

  GeneratedWorldRollbackVerification.Run(args[1], args[2]);
  return;
}

if (args.Length > 0 && args[0] == "--benchmark")
{
  if (args.Length != 2)
  {
    throw new ArgumentException("Usage: --benchmark <output-json-path>");
  }

  WriteComponentStoreBenchmarks(args[1]);
  return;
}

VerifyEntityAndComponentLifecycle();
VerifyQueryCandidateRevalidation();
VerifyMultiComponentEdit();
VerifyVersionedSnapshotConflicts();
VerifyReadInspectionDoesNotCommit();
VerifyRuntimeScopeAndIssuedIdentityHistory();
VerifyIdentityValueGuards();
VerifyEmptyIdentityCandidateCleanup();
VerifyGenerationExhaustionRetiresHandle();
VerifyOwnerThreadAccess();
SessionDisposalVerification.Run();
WorldSessionFailureVerification.Run();

Console.WriteLine("Entity organization verification passed.");

static void VerifyEntityAndComponentLifecycle()
{
  using var runtime = new EntityRuntime();
  RuntimeEntityHandle handle = runtime.CreateEntity();

  Assert(runtime.EntityCount == 1, "Creating an entity must register one constructing entity.");
  Assert(
    runtime.TryGetStatus(handle, out EntityRuntimeStatus status) &&
    status == EntityRuntimeStatus.Constructing,
    "New entities must remain constructing until their owner publishes them.");
  Assert(
    !runtime.TryGetReference(handle, EntityReferenceScope.Any, out _),
    "A constructing entity must not produce a normal runtime reference.");
  Assert(
    !runtime.TryCapture<EntityEcs.Components.EntityIdentityComponent, EntityUuid>(
      handle,
      identity => identity.Uuid,
      out _),
    "A constructing entity must not expose components through normal reads.");
  Assert(
    runtime.TryAttach(handle, new TestPositionComponent(2, 3)),
    "A component may be attached while an entity is being constructed.");
  Assert(
    runtime.TryAttach(handle, new TestHealthComponent(40)),
    "A second component type may be attached to the same entity.");
  var referenceComponent = new TestReferenceComponent();
  Assert(
    runtime.TryAttach(handle, referenceComponent),
    "A reference component instance may attach to its owning entity.");
  Assert(
    !runtime.TryAttach(handle, new TestHealthComponent(99)),
    "An entity may have only one component for an exact registered type.");
  Assert(runtime.TryPublishEntity(handle), "A valid constructing entity must publish once.");
  Assert(!runtime.TryPublishEntity(handle), "A running entity must not publish twice.");
  Assert(
    runtime.TryCapture<EntityEcs.Components.EntityIdentityComponent, EntityUuid>(
      handle,
      identity => identity.Uuid,
      out EntityUuid uuid) && uuid.IsAssigned,
    "The runtime must attach its root identity to the component store.");
  Assert(
    runtime.TryGetReference(handle, EntityReferenceScope.Npc, out EntityReference reference),
    "A running entity may expose a scoped reference.");
  Assert(
    runtime.TryResolve(reference, out RuntimeEntityHandle resolved) && resolved == handle,
    "A reference must resolve to its current runtime handle.");

  bool classStructureChangeWasRejected = false;
  Assert(
    runtime.TryEdit<TestReferenceComponent>(
      handle,
      (ref TestReferenceComponent component) =>
      {
        component.Value = 12;
        classStructureChangeWasRejected = !runtime.TryDetach<TestReferenceComponent>(handle);
        Assert(!runtime.TryBeginTermination(handle), "Termination must wait until a class edit ends.");
        Assert(
          !runtime.IsReadyForTermination(handle),
          "Termination readiness must be false while a component borrow is active.");
      }),
    "A class component must be edited through its owning runtime cell.");
  Assert(classStructureChangeWasRejected, "A class component cannot be detached while its cell is borrowed.");
  Assert(
    runtime.IsReadyForTermination(handle),
    "A running entity must become ready for termination after its borrow ends.");
  Assert(
    runtime.TryCapture<TestReferenceComponent, int>(
      handle,
      static component => component.Value,
    out int referenceValue) && referenceValue == 12,
    "A value snapshot must observe the class component edit without exposing its mutable instance.");

  bool mutableReferenceProjectionWasRejected = false;
  try
  {
    _ = runtime.TryCapture<TestReferenceComponent, TestReferenceSnapshot>(
      handle,
      static component => new TestReferenceSnapshot(component),
      out _);
  }
  catch (InvalidOperationException)
  {
    mutableReferenceProjectionWasRejected = true;
  }

  Assert(
    mutableReferenceProjectionWasRejected,
    "A public value snapshot must reject a struct that contains a mutable component reference.");

  bool reentrantEditWasRejected = false;
  Assert(
    runtime.TryCapture<TestReferenceComponent, int>(
      handle,
      component =>
      {
        reentrantEditWasRejected = !runtime.TryEdit<TestReferenceComponent>(
          handle,
          static (ref TestReferenceComponent nested) => nested.Value++);
        return component.Value;
      },
      out referenceValue),
    "A capture callback must complete while its entity is borrowed.");
  Assert(reentrantEditWasRejected, "A capture callback cannot reenter a write borrow for the same entity.");

  RuntimeEntityHandle second = runtime.CreateEntity();
  Assert(
    !runtime.TryAttach(second, referenceComponent),
    "One mutable reference component instance must not attach to two entities.");
  Assert(runtime.TryRemoveEntity(second), "An unpublished entity may be rolled back.");

  RuntimeEntityHandle isolated = runtime.CreateEntity();
  Assert(
    runtime.TryAttach(isolated, new TestReferenceComponent { Value = 3 }),
    "A distinct mutable component instance may attach to another entity.");
  Assert(runtime.TryPublishEntity(isolated), "The isolated component entity must publish.");
  Assert(
    runtime.TryEdit<TestReferenceComponent>(
      handle,
      static (ref TestReferenceComponent component) => component.Value = 17),
    "The first entity's class component must remain editable.");
  Assert(
    runtime.TryCapture<TestReferenceComponent, int>(
      isolated,
      static component => component.Value,
      out int isolatedValue) && isolatedValue == 3,
    "Distinct instances of the same class component must not share mutable state.");
  Assert(runtime.TryBeginTermination(isolated), "The isolated class component entity must terminate.");
  Assert(
    runtime.IsReadyForTermination(isolated),
    "A terminating, unborrowed entity must be ready for final removal.");
  Assert(runtime.TryRemoveEntity(isolated), "The isolated class component entity must be removed.");

  RuntimeEntityHandle[] matches = runtime.Match<TestPositionComponent, TestHealthComponent>();
  Assert(
    matches.Length == 1 && matches[0] == handle,
    "A component-combination query must return the entity whose attached cells match.");

  bool structuralChangeWasRejected = false;
  Assert(
    runtime.TryEdit<TestPositionComponent>(
      handle,
      (ref TestPositionComponent position) =>
      {
        position = position with { X = 8 };
        structuralChangeWasRejected = !runtime.TryDetach<TestPositionComponent>(handle);
        Assert(!runtime.TryBeginTermination(handle), "Termination must wait until a component edit ends.");
        bool disposeWasRejected = false;
        try
        {
          runtime.Dispose();
        }
        catch (InvalidOperationException)
        {
          disposeWasRejected = true;
        }

        Assert(disposeWasRejected, "Disposal must wait until all component borrows end.");
      }),
    "An attached struct component must be editable in its registered cell.");
  Assert(structuralChangeWasRejected, "Component structure must not change while its cell is borrowed.");
  RuntimeEntityHandle[] positionMatches = runtime.Match<TestPositionComponent>();
  Assert(
    positionMatches.Length == 1 && positionMatches[0] == handle &&
    runtime.TryCapture<TestPositionComponent, TestPositionSnapshot>(
      positionMatches[0],
      static position => new TestPositionSnapshot(position.X, position.Y),
      out TestPositionSnapshot positionSnapshot) &&
    positionSnapshot == new TestPositionSnapshot(8, 3),
    "A struct edit must be visible through a fresh component query.");

  Assert(
    runtime.TryCaptureVersioned<TestPositionComponent, TestPositionSnapshot>(
      handle,
      static position => new TestPositionSnapshot(position.X, position.Y),
      out EntityComponentSnapshot<TestPositionSnapshot> beforeReplace),
    "A component snapshot must capture both data and attachment revisions.");
  Assert(
    runtime.TryReplace(handle, beforeReplace, new TestPositionComponent(8, 3)),
    "A current versioned snapshot may commit a replacement.");
  Assert(
    !runtime.TryReplace(handle, beforeReplace, new TestPositionComponent(9, 3)),
    "A replacement must invalidate the snapshot that authorized it.");
  Assert(
    runtime.TryCaptureVersioned<TestPositionComponent, TestPositionSnapshot>(
      handle,
      static position => new TestPositionSnapshot(position.X, position.Y),
      out EntityComponentSnapshot<TestPositionSnapshot> afterReplace) &&
    afterReplace.AttachmentRevision > beforeReplace.AttachmentRevision,
    "Replacing a component must advance its attachment revision.");
  Assert(
    runtime.TryDetach<TestPositionComponent>(handle) &&
    runtime.TryAttach(handle, new TestPositionComponent(8, 3)),
    "A component may be detached and attached again on the same running entity.");
  Assert(
    !runtime.TryReplace(handle, afterReplace, new TestPositionComponent(10, 3)),
    "Detach and reattach must invalidate snapshots from the prior attachment.");
  Assert(
    runtime.TryCaptureVersioned<TestPositionComponent, TestPositionSnapshot>(
      handle,
      static position => new TestPositionSnapshot(position.X, position.Y),
      out EntityComponentSnapshot<TestPositionSnapshot> afterReattach) &&
    afterReattach.AttachmentRevision > afterReplace.AttachmentRevision,
    "A reattached component must have a newer attachment revision.");

  Assert(!runtime.Has<TestMissingComponent>(handle), "A missing component check must not create state.");
  Assert(
    !runtime.TryCapture<TestMissingComponent, TestPositionSnapshot>(handle, _ => default, out _),
    "Reading a missing component must fail without lazy initialization.");
  Assert(
    runtime.TryReplace(handle, new TestHealthComponent(30)),
    "An existing component may be replaced when no edit is active.");
  Assert(runtime.TryDetach<TestHealthComponent>(handle), "An attached component may be detached.");
  Assert(
    runtime.Match<TestPositionComponent, TestHealthComponent>().Length == 0,
    "Detaching a component must update the entity-side query membership.");

  Assert(runtime.TryBeginTermination(handle), "A running entity must enter termination once.");
  Assert(!runtime.TryResolve(reference, out _), "A terminating entity must not resolve for normal work.");
  Assert(!runtime.TryEdit<TestPositionComponent>(handle, static (ref TestPositionComponent _) => { }),
    "A terminating entity must not expose components for normal work.");
  Assert(runtime.TryRemoveEntity(handle), "A terminating entity must release its identity and cells.");
  Assert(!runtime.TryResolve(reference, out _), "A removed root reference must stay invalid.");
  Assert(!runtime.Has<TestPositionComponent>(handle), "A removed handle must not read a recycled cell.");

  RuntimeEntityHandle replacement = runtime.CreateEntity();
  Assert(
    replacement.LocalIndex == handle.LocalIndex && replacement.Generation == handle.Generation + 1,
    "Reusing a local slot must advance its generation.");
  Assert(runtime.TryPublishEntity(replacement), "A recreated entity must publish before normal reads.");
  Assert(
    runtime.TryCapture<EntityEcs.Components.EntityIdentityComponent, EntityUuid>(
      replacement,
      identity => identity.Uuid,
      out EntityUuid replacementUuid) && replacementUuid != uuid,
    "A recreated instance must receive a new identity root.");
  Assert(runtime.TryBeginTermination(replacement), "The recreated entity must enter termination.");
  Assert(runtime.TryRemoveEntity(replacement), "The recreated entity must release its runtime state.");
}

static void VerifyRuntimeScopeAndIssuedIdentityHistory()
{
  Guid repeatedUuid = Guid.Parse("10000000-0000-0000-0000-000000000001");
  Guid secondUuid = Guid.Parse("10000000-0000-0000-0000-000000000002");
  Guid replacementUuid = Guid.Parse("10000000-0000-0000-0000-000000000003");
  var generated = new Queue<Guid>([repeatedUuid, secondUuid, repeatedUuid, replacementUuid]);
  var registry = new EntityIdentityRegistry(() => generated.Dequeue());
  EntityReference oldReference;

  using (var firstRuntime = new EntityRuntime(registry))
  {
    RuntimeEntityHandle first = firstRuntime.CreateEntity();
    Assert(firstRuntime.TryPublishEntity(first), "The first runtime entity must publish.");
    Assert(
      firstRuntime.TryGetReference(first, EntityReferenceScope.Any, out oldReference),
      "The first runtime must expose its entity root.");

    using var secondRuntime = new EntityRuntime(registry);
    Assert(
      !secondRuntime.TryResolve(oldReference, out _),
      "A runtime must reject a reference issued by a different runtime.");
    RuntimeEntityHandle other = secondRuntime.CreateEntity();
    Assert(secondRuntime.TryPublishEntity(other), "A second runtime must publish its own entity.");
    Assert(
      first.LocalIndex == other.LocalIndex && first.Generation == other.Generation &&
      first.RuntimeId != other.RuntimeId,
      "Two runtimes may use the same local index and generation while keeping distinct handles.");
    Assert(
      !secondRuntime.TryGetStatus(first, out _) &&
      !secondRuntime.TryCapture<TestPositionComponent, float>(first, static value => value.X, out _),
      "A runtime must reject a foreign handle even when its local index and generation match.");

    Assert(firstRuntime.TryBeginTermination(first), "The first entity must terminate.");
    Assert(firstRuntime.TryRemoveEntity(first), "The first entity must be removed.");

    bool duplicateWasRejected = false;
    try
    {
      _ = secondRuntime.CreateEntity();
    }
    catch (InvalidOperationException)
    {
      duplicateWasRejected = true;
    }

    Assert(duplicateWasRejected, "An identity registry must reject a root it has already issued.");
    Assert(secondRuntime.EntityCount == 1, "A rejected duplicate root must not publish a partial entity.");
    Assert(
      !secondRuntime.TryResolve(oldReference, out _),
      "A removed root reference must stay invalid in every runtime.");

    RuntimeEntityHandle next = secondRuntime.CreateEntity();
    Assert(secondRuntime.TryPublishEntity(next), "The next unique root must still be usable.");
    Assert(
      secondRuntime.TryGetReference(next, EntityReferenceScope.Any, out EntityReference newReference) &&
      newReference.EntityId.Value == replacementUuid,
      "The registry must continue after rejecting a duplicate candidate.");
  }
}

static void VerifyIdentityValueGuards()
{
  Assert(!default(EntityUuid).IsAssigned, "The zero UUID value must remain unassigned.");
  Assert(!default(EntityRuntimeId).IsAssigned, "The zero runtime ID value must remain unassigned.");

  bool zeroUuidRejected = false;
  try
  {
    _ = new EntityUuid(Guid.Empty);
  }
  catch (ArgumentException)
  {
    zeroUuidRejected = true;
  }

  Assert(zeroUuidRejected, "Constructing an entity UUID from Guid.Empty must fail.");

  bool zeroRuntimeRejected = false;
  try
  {
    _ = new EntityRuntimeId(Guid.Empty);
  }
  catch (ArgumentException)
  {
    zeroRuntimeRejected = true;
  }

  Assert(zeroRuntimeRejected, "Constructing a runtime ID from Guid.Empty must fail.");

  bool zeroGenerationRejected = false;
  try
  {
    _ = new RuntimeEntityHandle(new EntityRuntimeId(Guid.NewGuid()), 0, 0);
  }
  catch (ArgumentOutOfRangeException)
  {
    zeroGenerationRejected = true;
  }

  Assert(zeroGenerationRejected, "A runtime handle must reject generation zero.");
}

static void VerifyEmptyIdentityCandidateCleanup()
{
  var candidates = new Queue<Guid>([Guid.Empty, Guid.NewGuid()]);
  var registry = new EntityIdentityRegistry(() => candidates.Dequeue());
  using var runtime = new EntityRuntime(registry);

  bool rejected = false;
  try
  {
    _ = runtime.CreateEntity();
  }
  catch (ArgumentException)
  {
    rejected = true;
  }

  Assert(rejected, "The identity registry must reject an empty root candidate.");
  Assert(runtime.EntityCount == 0, "An empty root candidate must not leave a partial entity.");

  RuntimeEntityHandle validHandle = runtime.CreateEntity();
  Assert(runtime.TryPublishEntity(validHandle), "A valid root must work after an empty candidate is rejected.");
  Assert(
    runtime.TryGetReference(validHandle, EntityReferenceScope.Any, out EntityReference reference) &&
    reference.EntityId.IsAssigned,
    "Only the valid candidate may become the published root.");
}

static void VerifyGenerationExhaustionRetiresHandle()
{
  using var runtime = new EntityRuntime();
  RuntimeEntityHandle first = runtime.CreateEntity();
  Assert(runtime.TryPublishEntity(first), "The initial entity must publish.");
  Assert(runtime.TryBeginTermination(first), "The initial entity must terminate.");
  Assert(runtime.TryRemoveEntity(first), "The initial entity must release its slot.");

  FieldInfo generationsField = typeof(EntityRuntime).GetField(
    "_generations",
    BindingFlags.Instance | BindingFlags.NonPublic) ??
    throw new InvalidOperationException("The runtime generation table could not be located.");
  var generations = generationsField.GetValue(runtime) as List<uint> ??
    throw new InvalidOperationException("The runtime generation table has an unexpected type.");
  generations[first.LocalIndex] = uint.MaxValue;

  RuntimeEntityHandle replacement = runtime.CreateEntity();
  Assert(
    replacement.LocalIndex != first.LocalIndex && replacement.Generation == 1,
    "A slot at the maximum generation must retire instead of wrapping to a stale handle.");
  Assert(runtime.TryPublishEntity(replacement), "A replacement on a fresh slot must publish.");
  Assert(
    !runtime.TryGetStatus(first, out _) && !runtime.Has<TestPositionComponent>(first),
    "The exhausted slot's old handle must remain invalid.");
  Assert(runtime.TryBeginTermination(replacement), "The replacement entity must terminate.");
  Assert(runtime.TryRemoveEntity(replacement), "The replacement entity must release its state.");
}

static void VerifyMultiComponentEdit()
{
  using var runtime = new EntityRuntime();
  RuntimeEntityHandle handle = runtime.CreateEntity();
  Assert(runtime.TryAttach(handle, new TestPositionComponent(2, 3)),
    "The first paired component must attach before publication.");
  Assert(runtime.TryAttach(handle, new TestHealthComponent(40)),
    "The second paired component must attach before publication.");
  Assert(runtime.TryAttach(handle, new TestVelocityComponent(1, 2)),
    "The third component must attach before a triple edit.");
  Assert(runtime.TryPublishEntity(handle), "A paired-edit entity must publish before edits.");

  bool structureChangeRejected = false;
  Assert(
    runtime.TryEditPair<TestPositionComponent, TestHealthComponent>(
      handle,
      (ref TestPositionComponent position, ref TestHealthComponent health) =>
      {
        position = position with { X = 8 };
        health = new TestHealthComponent(25);
        structureChangeRejected = !runtime.TryDetach<TestHealthComponent>(handle) &&
          !runtime.TryBeginTermination(handle);
      }),
    "A paired edit must update both attached cells during one borrow.");
  Assert(structureChangeRejected, "A paired borrow must block structural changes and termination.");
  Assert(
    runtime.TryCapture<TestPositionComponent, TestPositionSnapshot>(
      handle,
      static position => new TestPositionSnapshot(position.X, position.Y),
      out TestPositionSnapshot positionSnapshot) &&
    positionSnapshot == new TestPositionSnapshot(8, 3),
    "A paired edit must update the registered first component cell.");
  Assert(
    runtime.TryCapture<TestHealthComponent, int>(
      handle,
      static health => health.Value,
      out int healthValue) && healthValue == 25,
    "A paired edit must update the registered second component cell.");

  Assert(
    runtime.TryEditComponents<TestPositionComponent, TestHealthComponent, TestVelocityComponent>(
      handle,
      (ref TestPositionComponent position,
        ref TestHealthComponent health,
        ref TestVelocityComponent velocity) =>
      {
        position = position with { Y = 9 };
        health = new TestHealthComponent(21);
        velocity = new TestVelocityComponent(3, 4);
      }),
    "A triple edit must commit its three attached cells under one entity borrow.");
  Assert(
    runtime.TryCapture<TestPositionComponent, TestPositionSnapshot>(
      handle,
      static position => new TestPositionSnapshot(position.X, position.Y),
      out positionSnapshot) &&
    positionSnapshot == new TestPositionSnapshot(8, 9),
    "A triple edit must update the shared position component cell.");
  Assert(
    runtime.TryCapture<TestHealthComponent, int>(
      handle,
      static health => health.Value,
      out healthValue) && healthValue == 21,
    "A triple edit must update the shared health component cell.");
  Assert(
    runtime.TryCapture<TestVelocityComponent, TestVelocityComponent>(
      handle,
      static velocity => velocity,
      out TestVelocityComponent velocityValue) &&
    velocityValue == new TestVelocityComponent(3, 4),
    "A triple edit must update the shared third component cell.");

  bool missingComponentEditorInvoked = false;
  Assert(
    !runtime.TryEditPair<TestHealthComponent, TestMissingComponent>(
      handle,
      (ref TestHealthComponent health, ref TestMissingComponent missing) =>
      {
        missingComponentEditorInvoked = true;
        health = new TestHealthComponent(0);
      }) && !missingComponentEditorInvoked,
    "A paired edit must reject a missing component without invoking its editor.");
  Assert(
    runtime.TryCapture<TestHealthComponent, int>(
      handle,
      static health => health.Value,
      out healthValue) && healthValue == 21,
    "A rejected paired edit must leave the existing component unchanged.");

  Assert(runtime.TryBeginTermination(handle), "The paired-edit entity must terminate after borrowing ends.");
  Assert(runtime.TryRemoveEntity(handle), "The paired-edit entity must release its cells.");
}

static void VerifyQueryCandidateRevalidation()
{
  using var runtime = new EntityRuntime();
  RuntimeEntityHandle removed = runtime.CreateEntity();
  RuntimeEntityHandle retained = runtime.CreateEntity();
  Assert(runtime.TryAttach(removed, new TestPositionComponent(1, 1)), "The removable position must attach.");
  Assert(runtime.TryAttach(retained, new TestPositionComponent(2, 2)), "The retained position must attach.");
  Assert(runtime.TryPublishEntity(removed), "The removable entity must publish.");
  Assert(runtime.TryPublishEntity(retained), "The retained entity must publish.");

  RuntimeEntityHandle[] candidates = runtime.Match<TestPositionComponent>();
  var reusableCandidates = new List<RuntimeEntityHandle> { default };
  runtime.Match<TestPositionComponent>(reusableCandidates);
  Assert(
    candidates.Length == 2 && Array.IndexOf(candidates, removed) >= 0 &&
    Array.IndexOf(candidates, retained) >= 0 && reusableCandidates.Count == 2 &&
    reusableCandidates.Contains(removed) && reusableCandidates.Contains(retained),
    "The query must snapshot both candidate handles.");
  Assert(runtime.TryBeginTermination(removed), "The first query candidate must terminate.");
  Assert(runtime.TryRemoveEntity(removed), "The first query candidate must be removed.");
  bool removedCandidateRejected = false;
  bool retainedCandidateReadable = false;
  foreach (RuntimeEntityHandle candidate in candidates)
  {
    if (candidate == removed)
    {
      removedCandidateRejected = !runtime.TryCapture<TestPositionComponent, float>(
        candidate,
        static position => position.X,
        out _);
    }
    else if (candidate == retained)
    {
      retainedCandidateReadable = runtime.TryCapture<TestPositionComponent, float>(
        candidate,
        static position => position.X,
        out float retainedValue) && retainedValue == 2;
    }
  }

  Assert(removedCandidateRejected, "A stale query candidate must fail after deletion.");
  Assert(retainedCandidateReadable, "Deleting another candidate must not invalidate retained handles.");
  runtime.Match<TestPositionComponent>(reusableCandidates);
  Assert(
    reusableCandidates.Count == 1 && reusableCandidates[0] == retained,
    "A reused query buffer must clear prior candidates before capturing current membership.");
  Assert(runtime.TryBeginTermination(retained), "The retained entity must terminate.");
  Assert(runtime.TryRemoveEntity(retained), "The retained entity must be removed.");
}

static void VerifyVersionedSnapshotConflicts()
{
  using var runtime = new EntityRuntime();
  RuntimeEntityHandle handle = runtime.CreateEntity();
  Assert(runtime.TryAttach(handle, new TestPositionComponent(1, 2)), "A position must attach.");
  Assert(runtime.TryPublishEntity(handle), "The versioned-snapshot entity must publish.");
  Assert(
    runtime.TryCaptureVersioned<TestPositionComponent, TestPositionSnapshot>(
      handle,
      static position => new TestPositionSnapshot(position.X, position.Y),
      out EntityComponentSnapshot<TestPositionSnapshot> beforeEdit),
    "The test must capture the initial data revision.");
  Assert(
    runtime.TryEdit<TestPositionComponent>(
      handle,
      static (ref TestPositionComponent position) => position = position with { X = 4 }),
    "A write edit must update the component cell.");
  Assert(
    !runtime.TryReplace(handle, beforeEdit, new TestPositionComponent(9, 2)),
    "A data edit must invalidate an earlier snapshot before it can overwrite newer state.");
  bool editExceptionPropagated = false;
  try
  {
    _ = runtime.TryEdit<TestPositionComponent>(
      handle,
      (ref TestPositionComponent position) =>
      {
        position = position with { X = 6 };
        throw new InvalidOperationException("Expected verification callback failure.");
      });
  }
  catch (InvalidOperationException exception) when (
    exception.Message == "Expected verification callback failure.")
  {
    editExceptionPropagated = true;
  }

  Assert(editExceptionPropagated, "A component editor failure must propagate to its owner.");
  Assert(
    !runtime.TryReplace(handle, beforeEdit, new TestPositionComponent(9, 2)),
    "A failed editor must still invalidate snapshots because it may have partially changed state.");
  Assert(
    runtime.TryCaptureVersioned<TestPositionComponent, TestPositionSnapshot>(
      handle,
      static position => new TestPositionSnapshot(position.X, position.Y),
      out EntityComponentSnapshot<TestPositionSnapshot> current),
    "The updated component must remain capturable.");
  Assert(
    current.AttachmentRevision == beforeEdit.AttachmentRevision &&
    current.DataRevision > beforeEdit.DataRevision,
    "Data edits must advance data revision without changing attachment revision.");

  Assert(runtime.TryAttach(handle, new TestHealthComponent(10)), "Health must attach for paired-edit revision.");
  Assert(
    runtime.TryCaptureVersioned<TestHealthComponent, int>(
      handle,
      static health => health.Value,
      out EntityComponentSnapshot<int> beforePairEdit),
    "The health snapshot must be captured before paired edit.");
  Assert(
    runtime.TryEditPair<TestPositionComponent, TestHealthComponent>(
      handle,
      static (ref TestPositionComponent position, ref TestHealthComponent health) =>
      {
        position = position with { Y = 7 };
        health = new TestHealthComponent(8);
      }),
    "A paired edit must commit both component cells.");
  Assert(
    !runtime.TryReplace(handle, beforePairEdit, new TestHealthComponent(0)),
    "A paired edit must invalidate snapshots for either edited component.");
  Assert(runtime.TryBeginTermination(handle), "The versioned-snapshot entity must terminate.");
  Assert(runtime.TryRemoveEntity(handle), "The versioned-snapshot entity must be removed.");
}

static void VerifyReadInspectionDoesNotCommit()
{
  using var runtime = new EntityRuntime();
  RuntimeEntityHandle handle = runtime.CreateEntity();
  Assert(runtime.TryAttach(handle, new TestPositionComponent(3, 5)), "A position must attach.");
  Assert(runtime.TryPublishEntity(handle), "The inspected entity must publish.");
  Assert(
    runtime.TryCaptureVersioned<TestPositionComponent, TestPositionSnapshot>(
      handle,
      static position => new TestPositionSnapshot(position.X, position.Y),
      out EntityComponentSnapshot<TestPositionSnapshot> beforeInspection),
    "The component revision must be available before read inspection.");

  float inspectedX = 0;
  Assert(
    runtime.TryInspect<TestPositionComponent>(
      handle,
      (in TestPositionComponent position) => inspectedX = position.X),
    "A published component must support synchronous read inspection.");
  Assert(inspectedX == 3, "Read inspection must see the current component value.");
  bool reentrantTerminationRejected = false;
  Assert(
    runtime.TryInspect<TestPositionComponent>(
      handle,
      (in TestPositionComponent _) =>
        reentrantTerminationRejected = !runtime.TryBeginTermination(handle)),
    "Read inspection must retain its borrow for the full callback.");
  Assert(reentrantTerminationRejected,
    "Read inspection must reject structural changes during its callback.");
  Assert(
    runtime.TryCaptureVersioned<TestPositionComponent, TestPositionSnapshot>(
      handle,
      static position => new TestPositionSnapshot(position.X, position.Y),
      out EntityComponentSnapshot<TestPositionSnapshot> afterInspection),
    "The component revision must remain available after read inspection.");
  Assert(
    beforeInspection.DataRevision == afterInspection.DataRevision &&
    beforeInspection.AttachmentRevision == afterInspection.AttachmentRevision,
    "Read inspection must not commit a component revision.");

  Assert(runtime.TryBeginTermination(handle), "The inspected entity must terminate.");
  Assert(runtime.TryRemoveEntity(handle), "The inspected entity must be removed.");
}

static void VerifyOwnerThreadAccess()
{
  using var runtime = new EntityRuntime();
  RuntimeEntityHandle handle = runtime.CreateEntity();
  bool rejected = Task.Run(() =>
  {
    try
    {
      _ = runtime.TryGetStatus(handle, out _);
      return false;
    }
    catch (InvalidOperationException)
    {
      return true;
    }
  }).GetAwaiter().GetResult();

  Assert(rejected, "Runtime operations from a non-owner thread must be rejected.");
}

static void WriteComponentStoreBenchmarks(string outputPath)
{
  var cases = new[]
  {
    new { Name = "Projectile", Capacity = 1000 },
    new { Name = "NPC", Capacity = 200 },
    new { Name = "Player", Capacity = 255 },
  };
  var reports = new List<object>(cases.Length);
  foreach (var testCase in cases)
  {
    reports.Add(MeasureComponentStore(testCase.Name, testCase.Capacity));
  }

  var report = new
  {
    Benchmark = "EntityRuntime component access microbenchmark",
    Runtime = Environment.Version.ToString(),
    OperatingSystem = Environment.OSVersion.ToString(),
    ProcessorCount = Environment.ProcessorCount,
    Repetitions = 5,
    PassesPerRepetition = 100,
    Baseline = "Contiguous value-component array with the same slot count and initial values",
    RuntimePath = "EntityRuntime TryCapture/TryEdit and both allocating and reusable Match paths",
    Scenarios = reports,
    Limitation =
      "This isolates component-store access at domain slot capacities; it does not model full simulation phases.",
  };

  string fullPath = Path.GetFullPath(outputPath);
  Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
  File.WriteAllText(fullPath, JsonSerializer.Serialize(report, new JsonSerializerOptions
  {
    WriteIndented = true,
  }));
  Console.WriteLine($"Component-store benchmark written to {fullPath}");
}

static object MeasureComponentStore(string name, int capacity)
{
  const int passes = 100;
  const int repetitions = 5;
  var baseline = new TestPositionComponent[capacity];
  using var runtime = new EntityRuntime();
  var handles = new RuntimeEntityHandle[capacity];
  var reusableMatches = new List<RuntimeEntityHandle>(capacity);
  for (int index = 0; index < capacity; index++)
  {
    baseline[index] = new TestPositionComponent(index, index * 2);
    RuntimeEntityHandle handle = runtime.CreateEntity();
    Assert(runtime.TryAttach(handle, baseline[index]), "A benchmark component must attach.");
    Assert(runtime.TryPublishEntity(handle), "A benchmark entity must publish.");
    handles[index] = handle;
  }

  long LegacyRead()
  {
    long sum = 0;
    for (int pass = 0; pass < passes; pass++)
    {
      for (int index = 0; index < capacity; index++)
      {
        sum += (long)baseline[index].X;
      }
    }

    return sum;
  }

  long RuntimeRead()
  {
    long sum = 0;
    for (int pass = 0; pass < passes; pass++)
    {
      for (int index = 0; index < capacity; index++)
      {
        Assert(
          runtime.TryCapture<TestPositionComponent, float>(
            handles[index],
            static component => component.X,
            out float value),
          "A benchmark component must remain readable.");
        sum += (long)value;
      }
    }

    return sum;
  }

  long LegacyWrite()
  {
    for (int pass = 0; pass < passes; pass++)
    {
      for (int index = 0; index < capacity; index++)
      {
        TestPositionComponent value = baseline[index];
        baseline[index] = value with { X = value.X + 1 };
      }
    }

    return 0;
  }

  long RuntimeWrite()
  {
    for (int pass = 0; pass < passes; pass++)
    {
      for (int index = 0; index < capacity; index++)
      {
        Assert(
          runtime.TryEdit<TestPositionComponent>(
            handles[index],
            static (ref TestPositionComponent component) =>
              component = component with { X = component.X + 1 }),
          "A benchmark component must remain editable.");
      }
    }

    return 0;
  }

  long LegacyScan() => LegacyRead();

  long RuntimeScan()
  {
    long sum = 0;
    for (int pass = 0; pass < passes; pass++)
    {
      runtime.Match<TestPositionComponent>(reusableMatches);
      foreach (RuntimeEntityHandle handle in reusableMatches)
      {
        Assert(
          runtime.TryCapture<TestPositionComponent, float>(
            handle,
            static component => component.X,
            out float value),
          "A queried benchmark component must remain readable.");
        sum += (long)value;
      }
    }

    return sum;
  }

  long RuntimeAllocatingScan()
  {
    long sum = 0;
    for (int pass = 0; pass < passes; pass++)
    {
      foreach (RuntimeEntityHandle handle in runtime.Match<TestPositionComponent>())
      {
        Assert(
          runtime.TryCapture<TestPositionComponent, float>(
            handle,
            static component => component.X,
            out float value),
          "A queried benchmark component must remain readable.");
        sum += (long)value;
      }
    }

    return sum;
  }

  _ = LegacyRead();
  _ = RuntimeRead();
  _ = LegacyWrite();
  _ = RuntimeWrite();
  _ = LegacyScan();
  _ = RuntimeScan();
  _ = RuntimeAllocatingScan();

  var measurements = new
  {
    Read = new
    {
      Baseline = Measure(LegacyRead, capacity, passes, repetitions),
      EntityRuntime = Measure(RuntimeRead, capacity, passes, repetitions),
    },
    Write = new
    {
      Baseline = Measure(LegacyWrite, capacity, passes, repetitions),
      EntityRuntime = Measure(RuntimeWrite, capacity, passes, repetitions),
    },
    Scan = new
    {
      Baseline = Measure(LegacyScan, capacity, passes, repetitions),
      EntityRuntimeReusableBuffer = Measure(RuntimeScan, capacity, passes, repetitions),
      EntityRuntimeAllocatingArray = Measure(RuntimeAllocatingScan, capacity, passes, repetitions),
    },
  };
  long legacySum = baseline.Sum(static component => (long)component.X);
  long runtimeSum = 0;
  foreach (RuntimeEntityHandle handle in runtime.Match<TestPositionComponent>())
  {
    Assert(
      runtime.TryCapture<TestPositionComponent, float>(
        handle,
        static component => component.X,
        out float value),
      "A final benchmark component must remain readable.");
    runtimeSum += (long)value;
  }

  Assert(legacySum == runtimeSum, "The benchmark paths must preserve equivalent component values.");
  return new
  {
    EntityClass = name,
    SlotCount = capacity,
    Measurements = measurements,
    FinalValueChecksum = legacySum,
  };
}

static object Measure(Func<long> operation, int capacity, int passes, int repetitions)
{
  var elapsedTicks = new long[repetitions];
  var allocatedBytes = new long[repetitions];
  long checksum = 0;
  for (int repetition = 0; repetition < repetitions; repetition++)
  {
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
    long allocationStart = GC.GetAllocatedBytesForCurrentThread();
    long startTimestamp = Stopwatch.GetTimestamp();
    checksum = operation();
    elapsedTicks[repetition] = Stopwatch.GetTimestamp() - startTimestamp;
    allocatedBytes[repetition] = GC.GetAllocatedBytesForCurrentThread() - allocationStart;
  }

  Array.Sort(elapsedTicks);
  Array.Sort(allocatedBytes);
  long medianTicks = elapsedTicks[repetitions / 2];
  long medianAllocatedBytes = allocatedBytes[repetitions / 2];
  long entityVisits = (long)capacity * passes;
  return new
  {
    MedianMilliseconds = medianTicks * 1000.0 / Stopwatch.Frequency,
    NanosecondsPerEntityVisit = medianTicks * 1_000_000_000.0 / Stopwatch.Frequency / entityVisits,
    MedianAllocatedBytes = medianAllocatedBytes,
    AllocatedBytesPerEntityVisit = medianAllocatedBytes / (double)entityVisits,
    Checksum = checksum,
  };
}

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

internal record struct TestPositionComponent(float X, float Y);

internal readonly record struct TestHealthComponent(int Value);

internal readonly record struct TestVelocityComponent(float X, float Y);

internal readonly record struct TestPositionSnapshot(float X, float Y);

internal readonly record struct TestReferenceSnapshot(TestReferenceComponent Component);

internal readonly record struct TestMissingComponent;

internal sealed class TestReferenceComponent
{
  public int Value { get; set; }
}
