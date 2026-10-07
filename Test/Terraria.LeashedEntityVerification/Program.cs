using EntityEcs;
using EntityEcs.Components;
using Terraria.Items;
using Terraria.LeashedEntity;
using Terraria.Relationships;
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

static LeashedDefinitionCatalog CreateCatalog()
{
  LeashedDefinitionCatalog catalog = new();
  catalog.Register("kite", LeashedDefinitionKind.Kite, contentId: 9001);
  catalog.Register("walker", LeashedDefinitionKind.Critter, contentId: 9002);
  catalog.Freeze();
  return catalog;
}

static LeashedEntityRegistrationSystem CreateSystem(EntityRuntime runtime)
{
  return new LeashedEntityRegistrationSystem(runtime, CreateCatalog());
}

static LeashedEntityRegistrationSnapshot Register(
  LeashedEntityRegistrationSystem system,
  SectionCoordinate section,
  bool sectionActive = true,
  int definitionId = 2)
{
  return system.Register(new(definitionId, section, sectionActive));
}

static EntityReference CreateExternalRoot(EntityRuntime runtime)
{
  RuntimeEntityHandle handle = runtime.CreateEntity();
  Require(runtime.TryPublishEntity(handle), "An external anchor root must publish.");
  Require(runtime.TryGetReference(handle, EntityReferenceScope.Any, out EntityReference reference),
    "An external anchor root must expose its scoped runtime reference.");
  return reference;
}

static EntityReference[] CaptureAnchorMembers(
  EntityRuntime runtime,
  EntityReference anchorReference)
{
  if (!runtime.TryResolve(anchorReference, out RuntimeEntityHandle handle))
  {
    return Array.Empty<EntityReference>();
  }

  EntityReference[]? members = null;
  bool found = runtime.TryInspect<LeashedAnchorLinksComponent>(
    handle,
    (in LeashedAnchorLinksComponent component) => members = component.CopyMembers());
  return found ? members! : Array.Empty<EntityReference>();
}

static LeashedEntityAnchorRelationComponent CaptureAnchorRelation(
  EntityRuntime runtime,
  EntityReference memberReference)
{
  Require(runtime.TryResolve(memberReference, out RuntimeEntityHandle handle),
    "A Leashed member must resolve before relation capture.");
  Require(
    runtime.TryCapture<
      LeashedEntityAnchorRelationComponent,
      LeashedEntityAnchorRelationComponent>(
        handle,
        static relation => relation,
        out LeashedEntityAnchorRelationComponent relation),
    "A Leashed member must own its anchor relation component.");
  return relation;
}

static void VerifyDefinitionCatalog()
{
  LeashedDefinitionCatalog catalog = new();
  LeashedDefinitionDescriptor kite =
    catalog.Register("kite", LeashedDefinitionKind.Kite, contentId: 9001);
  LeashedDefinitionDescriptor walker =
    catalog.Register("walker", LeashedDefinitionKind.Critter, contentId: 9002);

  Require(kite.DefinitionId == 1, "The first definition must preserve the null sentinel.");
  Require(walker.DefinitionId == 2, "Definitions must receive ordered ids.");

  LeashedDefinitionQuery query = new(catalog);
  Require(
    query.TryGetByKey("walker", out LeashedDefinitionDescriptor resolved),
    "The key query must resolve.");
  Require(
    resolved.DefinitionId == walker.DefinitionId,
    "The key query must return the registered definition.");
  Require(query.TryGetById(kite.DefinitionId, out _), "The id query must resolve.");
  Require(!query.TryGetById(0, out _), "Definition id zero is the unbound sentinel.");

  catalog.Freeze();
  RequireThrows<InvalidOperationException>(
    () => catalog.Register("late", LeashedDefinitionKind.Critter),
    "A frozen catalog must reject late registration.");
}

static void VerifyRuntimeRootAndSlotReuse()
{
  using EntityRuntime runtime = new();
  using EntityRuntime otherRuntime = new();
  LeashedEntityRegistrationSystem system = CreateSystem(runtime);
  SectionCoordinate section = new(2, 3);

  LeashedEntityRegistrationSnapshot first = Register(system, section);
  Require(first.Handle.LegacySlot == 0, "The first entity must receive slot zero.");
  Require(first.Handle.SlotGeneration == 1, "The first slot generation must be one.");
  Require(
    first.Lifecycle.IsActive && first.Lifecycle.Spawned,
    "An active section must spawn the entity.");
  Require(system.TryGetByLegacySlot(0, 1, out _), "The assigned slot must be queryable.");
  Require(runtime.TryResolve(first.RuntimeEntityReference, out RuntimeEntityHandle firstRoot),
    "The registration reference must resolve through the injected runtime.");
  Require(runtime.Has<LeashedEntityStateComponent>(firstRoot)
      && runtime.Has<LeashedEntityLegacySlotComponent>(firstRoot)
      && runtime.Has<LeashedEntityLifecycleComponent>(firstRoot)
      && runtime.Has<LeashedEntitySectionMembershipComponent>(firstRoot)
      && runtime.Has<LeashedEntityAnchorRelationComponent>(firstRoot),
    "Registration capabilities must live on the runtime root.");
  Require(!otherRuntime.TryResolve(first.RuntimeEntityReference, out _),
    "A reference from one runtime must not resolve in another.");

  Require(system.Remove(first.Handle), "The registered entity must be removable.");
  Require(!runtime.TryResolve(first.RuntimeEntityReference, out _),
    "Removal must terminate and remove the runtime root.");
  Require(!system.TryGetByLegacySlot(0, 1, out _), "A removed slot generation must be stale.");

  LeashedEntityRegistrationSnapshot replacement = Register(system, section);
  Require(replacement.Handle.LegacySlot == 0, "A removed slot must be reusable.");
  Require(replacement.Handle.SlotGeneration == 2, "Slot reuse must increment the generation.");
  Require(replacement.RuntimeEntityReference.EntityId != first.RuntimeEntityReference.EntityId,
    "Reusing a legacy slot must create a distinct runtime root identity.");
  Require(!system.TryGet(first.RuntimeEntityReference, out _),
    "A removed runtime reference must no longer query a registration.");
  Require(!system.Remove(first.Handle)
      && runtime.TryResolve(replacement.RuntimeEntityReference, out _)
      && system.TryGetByLegacySlot(
        replacement.Handle.LegacySlot,
        replacement.Handle.SlotGeneration,
        out _),
    "A stale handle must not remove a new root that reused its legacy slot.");
}

static void VerifySectionActivationAndCompaction()
{
  using EntityRuntime runtime = new();
  LeashedEntityRegistrationSystem system = CreateSystem(runtime);
  SectionCoordinate section = new(5, 7);
  LeashedEntityRegistrationSnapshot first = Register(system, section, sectionActive: true);
  LeashedEntityRegistrationSnapshot second = Register(system, section, sectionActive: false);
  LeashedEntityRegistrationSnapshot third = Register(system, section, sectionActive: false);
  LeashedEntityRegistrationSnapshot fourth = Register(system, section, sectionActive: false);

  Require(second.Lifecycle.Spawned && third.Lifecycle.Spawned,
    "New members must observe existing section activity.");
  Require(
    system.SetSectionActive(section, true) == 4,
    "Section activation must update all members.");
  Require(system.TryGet(
      third.RuntimeEntityReference,
      out LeashedEntityRegistrationSnapshot activeThird)
      && activeThird.Lifecycle.Spawned,
    "Section activation must mark members spawned.");

  Require(system.Remove(second.Handle), "The middle member must be removable.");
  Require(system.Sections.TryGetSnapshot(section, out LeashedSectionSnapshot withHole)
      && withHole.EmptySlots == 1,
    "A single removal must preserve a reusable hole before compaction.");
  Require(system.Remove(third.Handle), "A second member must be removable.");
  Require(system.Sections.TryGetSnapshot(section, out LeashedSectionSnapshot compacted),
    "The section snapshot must exist.");
  Require(compacted.ActiveCount == 2 && compacted.EmptySlots == 0,
    "Removal must compact a half-empty section.");
  Require(system.TryGet(fourth.RuntimeEntityReference, out LeashedEntityRegistrationSnapshot moved)
      && moved.SectionMembership.SectionSlot == 1,
    "Compaction must update the moved entity's section slot.");

  Require(system.SetSectionActive(section, false) == 2,
    "Section deactivation must update the remaining members.");
  Require(system.TryGet(
      first.RuntimeEntityReference,
      out LeashedEntityRegistrationSnapshot inactiveFirst)
      && !inactiveFirst.Lifecycle.Spawned,
    "Section deactivation must stop spawning without removing the entity.");
  EntityReference firstReference = inactiveFirst.RuntimeEntityReference;
  Require(system.SetSectionActive(section, true) == 2,
    "A deactivated section must be able to reactivate its members.");
  Require(system.TryGet(firstReference, out LeashedEntityRegistrationSnapshot reactivated)
      && reactivated.RuntimeEntityReference == firstReference
      && reactivated.Lifecycle.Spawned,
    "Section activation must retain the same runtime root.");
}

static void VerifySectionHoleReuse()
{
  using EntityRuntime runtime = new();
  LeashedEntityRegistrationSystem system = CreateSystem(runtime);
  SectionCoordinate section = new(8, 9);
  LeashedEntityRegistrationSnapshot first = Register(system, section);
  LeashedEntityRegistrationSnapshot second = Register(system, section);
  Register(system, section);
  Register(system, section);

  Require(
    system.Remove(first.Handle),
    "The first member must be removable without forced compaction.");
  Require(system.Sections.TryGetSnapshot(section, out LeashedSectionSnapshot beforeReuse)
      && beforeReuse.EmptySlots == 1
      && beforeReuse.SlotCount == 4,
    "The removed section slot must remain available for reuse.");

  LeashedEntityRegistrationSnapshot replacement = Register(system, section);
  Require(replacement.SectionMembership.SectionSlot == first.SectionMembership.SectionSlot,
    "A new member must reuse the removed section slot.");
  Require(system.Sections.TryGetSnapshot(section, out LeashedSectionSnapshot afterReuse)
      && afterReuse.EmptySlots == 0
      && afterReuse.SlotCount == 4,
    "Reusing a section slot must not grow the logical bucket.");
  Require(system.TryGet(
      second.RuntimeEntityReference,
      out LeashedEntityRegistrationSnapshot retained)
      && retained.SectionMembership.SectionSlot == second.SectionMembership.SectionSlot,
    "Reusing a hole must preserve existing member slots.");
}

static void VerifyBorrowedOperationsAreRejectedAtomically()
{
  using EntityRuntime runtime = new();
  LeashedEntityRegistrationSystem system = CreateSystem(runtime);
  SectionCoordinate section = new(3, 4);
  LeashedEntityRegistrationSnapshot member = Register(system, section, sectionActive: true);
  EntityReference oldAnchor = CreateExternalRoot(runtime);
  EntityReference newAnchor = CreateExternalRoot(runtime);
  Require(
    system.SetAnchor(
      member.Handle,
      oldAnchor,
      new TileCoordinate(20, 30),
      new TileEntityId(4),
      1),
    "The member must attach to its first anchor.");

  Require(runtime.TryResolve(oldAnchor, out RuntimeEntityHandle oldAnchorHandle),
    "The original anchor must remain live for the borrow check.");
  bool oldAnchorRejectedChange = true;
  Require(runtime.TryInspect<LeashedAnchorLinksComponent>(
    oldAnchorHandle,
    (in LeashedAnchorLinksComponent _) =>
    {
      oldAnchorRejectedChange = !system.SetAnchor(member.Handle, newAnchor);
    }),
    "The original anchor reverse link must be inspectable.");
  Require(oldAnchorRejectedChange,
    "A borrowed old anchor must reject relation replacement before any side commits.");
  Require(
    CaptureAnchorRelation(runtime, member.RuntimeEntityReference).AnchorReference == oldAnchor,
    "A rejected replacement must preserve the forward relation.");
  Require(CaptureAnchorMembers(runtime, oldAnchor).Contains(member.RuntimeEntityReference)
      && !CaptureAnchorMembers(runtime, newAnchor).Contains(member.RuntimeEntityReference),
    "A rejected replacement must preserve both reverse-link sets.");
  Require(runtime.TryResolve(oldAnchor, out _) && runtime.TryResolve(newAnchor, out _)
      && system.TryGetByLegacySlot(member.Handle.LegacySlot, member.Handle.SlotGeneration, out _)
      && system.Sections.TryGetSnapshot(section, out LeashedSectionSnapshot oldAnchorSection)
      && oldAnchorSection.ActiveCount == 1
      && system.ActiveCount == 1,
    "A rejected relation replacement must preserve roots, slot and section projections.");

  Require(runtime.TryResolve(member.RuntimeEntityReference, out RuntimeEntityHandle memberHandle),
    "The member root must remain live for the borrow check.");
  bool borrowedMemberRejectedTargetRemoval = false;
  int clearedAnchorLinkCount = -1;
  Require(runtime.TryInspect<LeashedEntityAnchorRelationComponent>(
    memberHandle,
    (in LeashedEntityAnchorRelationComponent borrowedRelation) =>
    {
      borrowedMemberRejectedTargetRemoval = !system.TryRemoveAnchorTarget(
        oldAnchor,
        out clearedAnchorLinkCount);
    }),
    "The member relation must be inspectable.");
  Require(borrowedMemberRejectedTargetRemoval && clearedAnchorLinkCount == 0,
    "A borrowed member must reject target removal before its reverse relation is cleared.");
  Require(CaptureAnchorRelation(runtime, member.RuntimeEntityReference).AnchorReference == oldAnchor
      && CaptureAnchorMembers(runtime, oldAnchor).Contains(member.RuntimeEntityReference),
    "A rejected target removal must preserve both relation directions.");
  Require(runtime.TryResolve(member.RuntimeEntityReference, out _)
      && system.TryGetByLegacySlot(member.Handle.LegacySlot, member.Handle.SlotGeneration, out _)
      && system.Sections.TryGetSnapshot(section, out LeashedSectionSnapshot borrowedMemberSection)
      && borrowedMemberSection.Handles.Contains(member.Handle),
    "A rejected target removal must preserve its runtime root, slot and section entry.");

  bool borrowedMemberRejectedSectionChange = false;
  int updatedSectionMemberCount = -1;
  Require(runtime.TryInspect<LeashedEntityLifecycleComponent>(
    memberHandle,
    (in LeashedEntityLifecycleComponent borrowedLifecycle) =>
    {
      borrowedMemberRejectedSectionChange = !system.TrySetSectionActive(
        section,
        false,
        out updatedSectionMemberCount);
    }),
    "The lifecycle component must be inspectable.");
  Require(borrowedMemberRejectedSectionChange && updatedSectionMemberCount == 0,
    "A borrowed section member must reject the section transition before index changes.");
  Require(system.Sections.TryGetSnapshot(section, out LeashedSectionSnapshot unchangedSection)
      && unchangedSection.IsActive,
    "A rejected section change must preserve the section-index activity.");
  Require(system.TryGet(
      member.RuntimeEntityReference,
      out LeashedEntityRegistrationSnapshot unchangedMember)
      && unchangedMember.Lifecycle.Spawned
      && unchangedMember.SectionMembership.IsSectionActive,
    "A rejected section change must preserve lifecycle and membership components.");
  Require(runtime.TryResolve(member.RuntimeEntityReference, out _)
      && CaptureAnchorRelation(runtime, member.RuntimeEntityReference).AnchorReference == oldAnchor
      && CaptureAnchorMembers(runtime, oldAnchor).Contains(member.RuntimeEntityReference)
      && system.TryGetByLegacySlot(member.Handle.LegacySlot, member.Handle.SlotGeneration, out _),
    "A rejected section change must preserve root, slot and both anchor directions.");
}

static void VerifyTargetAndMemberRemovalCleanBothDirections()
{
  using EntityRuntime runtime = new();
  LeashedEntityRegistrationSystem system = CreateSystem(runtime);
  LeashedEntityRegistrationSnapshot member = Register(system, new(4, 6));
  EntityReference target = CreateExternalRoot(runtime);
  Require(
    system.SetAnchor(
      member.Handle,
      target,
      new TileCoordinate(12, 18),
      new TileEntityId(9),
      7),
    "The Leashed root must attach to an external anchor root.");
  Require(CaptureAnchorMembers(runtime, target).Contains(member.RuntimeEntityReference),
    "The target root must store the reverse relation component.");

  Require(runtime.TryResolve(target, out RuntimeEntityHandle targetHandle)
      && runtime.TryBeginTermination(targetHandle)
      && runtime.TryRemoveEntity(targetHandle),
    "The external anchor owner must be able to remove its root.");
  Require(system.RemoveAnchorTarget(target) == 1,
    "Target removal must clear reverse-projected members after root deletion.");
  Require(CaptureAnchorRelation(runtime, member.RuntimeEntityReference).AnchorReference is null,
    "Target removal must clear the member's live anchor reference.");
  Require(system.TryGet(
      member.RuntimeEntityReference,
      out LeashedEntityRegistrationSnapshot detached)
      && detached.AnchorRelation.AnchorCoordinate == new TileCoordinate(12, 18)
      && detached.AnchorRelation.PersistentAnchorId == new TileEntityId(9),
    "Target removal must retain the durable coordinate and anchor-id projections.");

  EntityReference secondTarget = CreateExternalRoot(runtime);
  Require(
    system.SetAnchor(member.Handle, secondTarget),
    "A detached member must accept a replacement target.");
  Require(
    system.Remove(member.Handle),
    "Removing a Leashed root must clean its outgoing relation.");
  Require(!runtime.TryResolve(member.RuntimeEntityReference, out _),
    "Removing a Leashed registration must remove its runtime root.");
  Require(!CaptureAnchorMembers(runtime, secondTarget).Contains(member.RuntimeEntityReference),
    "Removing a Leashed root must clean the target's reverse component.");

  LeashedEntityRegistrationSnapshot worldMember = Register(system, new(1, 2));
  Require(system.Clear() == 1, "World unload must report and remove all remaining Leashed roots.");
  Require(system.ActiveCount == 0 && system.Sections.SectionCount == 0,
    "World unload must clear slot and section projections.");
  Require(!runtime.TryResolve(worldMember.RuntimeEntityReference, out _),
    "World unload must remove the remaining Leashed runtime root.");
  Require(runtime.EntityCount == 1,
    "World unload must leave unrelated roots in the injected runtime alive.");
}

static void VerifyTileEntityAndLeashedShareOneRuntime()
{
  using EntityRuntime runtime = new();
  TileEntityUpdateSchedule updates = new();
  TileEntityStore tileEntities = new(runtime, updates);
  TileEntitySnapshot tileSnapshot = new(
    new TileEntityId(0),
    new TileEntityTypeId(0),
    new TileCoordinate(30, 40),
    Array.Empty<ItemState>());
  TileEntityRestoreSystem.Apply(tileEntities, new[] { tileSnapshot }, nextId: 1);
  Require(
    tileEntities.TryGetEntityReference(new TileEntityId(0), out EntityReference tileReference),
    "A TileEntity root must be created in the session runtime.");

  LeashedEntityRegistrationSystem leashedEntities = CreateSystem(runtime);
  LeashedEntityRegistrationSnapshot leashed = Register(leashedEntities, new(3, 5));
  Require(tileReference.RuntimeId == leashed.RuntimeEntityReference.RuntimeId
      && tileReference.EntityId != leashed.RuntimeEntityReference.EntityId
      && runtime.TryResolve(tileReference, out _)
      && runtime.TryResolve(leashed.RuntimeEntityReference, out _),
    "TileEntity and Leashed registrations must resolve as distinct roots in the same runtime.");
  Require(leashedEntities.SetAnchor(leashed.Handle, tileReference),
    "A Leashed root must accept a TileEntity runtime reference as its anchor.");
  Require(CaptureAnchorMembers(runtime, tileReference).Contains(leashed.RuntimeEntityReference),
    "A TileEntity root must hold the reverse Leashed anchor relation.");

  Require(
    tileEntities.Remove(new TileEntityId(0)),
    "The TileEntity owner must remove its runtime root.");
  Require(leashedEntities.RemoveAnchorTarget(tileReference) == 1,
    "Leashed must clear its member relation when the TileEntity target has been removed.");
  Require(CaptureAnchorRelation(runtime, leashed.RuntimeEntityReference).AnchorReference is null,
    "The Leashed member must no longer retain the removed TileEntity reference.");
}

static void VerifyNetworkValidationAndCommand()
{
  using EntityRuntime runtime = new();
  LeashedDefinitionCatalog catalog = CreateCatalog();
  LeashedEntityRegistrationSystem system = new(runtime, catalog);
  LeashedEntityNetworkAdapter adapter = new();
  SectionCoordinate section = new(4, 6);
  LeashedEntityRegistrationSnapshot current = Register(system, section);

  LeashedNetworkFrame unknownDefinition =
    new(LeashedNetworkFrameKind.FullSync, 3, 0, 99, section);
  Require(adapter.Validate(unknownDefinition, catalog, system).Status
    == LeashedNetworkFrameValidationStatus.RejectedUnknownDefinition,
    "A full sync with an unknown definition must be rejected.");

  LeashedNetworkFrame invalidSlot =
    new(LeashedNetworkFrameKind.FullSync, -1, 0, 2, section);
  Require(adapter.Validate(invalidSlot, catalog, system).Status
    == LeashedNetworkFrameValidationStatus.RejectedInvalidSlot,
    "A negative legacy slot must be rejected.");

  LeashedNetworkFrame staleGeneration =
    new(
      LeashedNetworkFrameKind.PartialSync,
      current.Handle.LegacySlot,
      current.Handle.SlotGeneration + 1,
      2,
      section);
  Require(adapter.Validate(staleGeneration, catalog, system).Status
    == LeashedNetworkFrameValidationStatus.RejectedStaleGeneration,
    "A partial sync with a stale generation must be rejected.");

  LeashedNetworkFrame typeMismatch =
    new(
      LeashedNetworkFrameKind.PartialSync,
      current.Handle.LegacySlot,
      current.Handle.SlotGeneration,
      1,
      section);
  Require(adapter.Validate(typeMismatch, catalog, system).Status
    == LeashedNetworkFrameValidationStatus.RejectedTypeMismatch,
    "A partial sync with a different definition must be rejected.");

  LeashedNetworkFrame sectionMismatch =
    new(
      LeashedNetworkFrameKind.FullSync,
      current.Handle.LegacySlot,
      current.Handle.SlotGeneration,
      2,
      new(4, 7));
  Require(adapter.Validate(sectionMismatch, catalog, system).Status
    == LeashedNetworkFrameValidationStatus.RejectedSectionMismatch,
    "A full sync with a different section must be rejected.");

  LeashedNetworkFrame fullRegistration =
    new(LeashedNetworkFrameKind.FullSync, 5, 0, 2, section);
  Require(adapter.TryValidateAndCreateCommand(
      fullRegistration,
      catalog,
      system,
      out LeashedNetworkFrameCommand fullCommand,
      out LeashedNetworkFrameValidation fullValidation)
    && fullValidation.RequiresRegistration
    && fullCommand.RequiresRegistration
    && fullCommand.ExistingHandle is null,
    "An accepted full sync for an empty slot must produce a registration command.");

  LeashedNetworkFrame remove =
    new(
      LeashedNetworkFrameKind.Remove,
      current.Handle.LegacySlot,
      current.Handle.SlotGeneration,
      0,
      section);
  Require(adapter.TryValidateAndCreateCommand(
      remove,
      catalog,
      system,
      out LeashedNetworkFrameCommand removeCommand,
      out LeashedNetworkFrameValidation removeValidation)
    && !removeCommand.RequiresRegistration
    && removeCommand.ExistingHandle == current.Handle
    && removeValidation.ExistingHandle == current.Handle,
    "A remove frame must produce a command tied to the current generation.");

  LeashedNetworkFrame missingRemove =
    new(LeashedNetworkFrameKind.Remove, 6, 1, 0, section);
  Require(adapter.Validate(missingRemove, catalog, system).Status
    == LeashedNetworkFrameValidationStatus.RejectedMissingEntity,
    "A remove frame for a missing slot must be rejected.");

  LeashedNetworkFrame missingPartial =
    new(LeashedNetworkFrameKind.PartialSync, 6, 1, 2, section);
  Require(adapter.Validate(missingPartial, catalog, system).Status
    == LeashedNetworkFrameValidationStatus.RejectedMissingEntity,
    "A partial sync for a missing slot must be rejected.");

  LeashedNetworkFrame partialWithoutGeneration =
    new(LeashedNetworkFrameKind.PartialSync, current.Handle.LegacySlot, 0, 2, section);
  Require(adapter.Validate(partialWithoutGeneration, catalog, system).Status
    == LeashedNetworkFrameValidationStatus.RejectedStaleGeneration,
    "A partial sync without the current generation must be rejected.");
  Require(system.ActiveCount == 1,
    "Network validation and command creation must not mutate registration authority.");
}

static void VerifyClearPreservesGenerationHistory()
{
  using EntityRuntime runtime = new();
  LeashedEntityRegistrationSystem system = CreateSystem(runtime);
  SectionCoordinate section = new(1, 1);
  LeashedEntityRegistrationSnapshot beforeClear = Register(system, section, definitionId: 1);
  EntityReference anchorReference = CreateExternalRoot(runtime);
  Require(system.SetAnchor(beforeClear.Handle, anchorReference),
    "The root must have a live external relation before the clear borrow check.");

  Require(runtime.TryResolve(anchorReference, out RuntimeEntityHandle anchorHandle),
    "The anchor root must resolve for the clear borrow check.");
  int rejectedClearCount = -1;
  bool clearRejected = false;
  Require(runtime.TryInspect<LeashedAnchorLinksComponent>(
    anchorHandle,
    (in LeashedAnchorLinksComponent _) =>
    {
      clearRejected = !system.TryClear(out rejectedClearCount);
    }),
    "The reverse-link component must be inspectable during the clear borrow check.");
  Require(clearRejected && rejectedClearCount == 0,
    "Clear must reject a borrowed anchor before removing any registration.");
  Require(runtime.TryResolve(beforeClear.RuntimeEntityReference, out _)
      && system.TryGetByLegacySlot(
        beforeClear.Handle.LegacySlot,
        beforeClear.Handle.SlotGeneration,
        out _)
      && CaptureAnchorRelation(
        runtime,
        beforeClear.RuntimeEntityReference).AnchorReference == anchorReference
      && CaptureAnchorMembers(runtime, anchorReference).Contains(beforeClear.RuntimeEntityReference)
      && system.Sections.TryGetSnapshot(section, out LeashedSectionSnapshot unchangedSection)
      && unchangedSection.Handles.Contains(beforeClear.Handle),
    "A rejected clear must preserve roots, both relation directions, slot and section state.");

  Require(system.Clear() == 1, "Clear must report the number of active entities.");
  Require(system.ActiveCount == 0, "Clear must remove all runtime registrations.");
  Require(!system.TryGet(beforeClear.RuntimeEntityReference, out _),
    "Cleared runtime references must no longer resolve through the system.");
  Require(!runtime.TryResolve(beforeClear.RuntimeEntityReference, out _),
    "Clear must remove registered roots from the injected runtime.");
  Require(
    !CaptureAnchorMembers(runtime, anchorReference)
      .Contains(beforeClear.RuntimeEntityReference),
    "Clear must remove the member from its external anchor's reverse links.");
  Require(!system.Sections.TryGetSnapshot(section, out _),
    "Clear must remove rebuildable section indexes.");

  LeashedEntityRegistrationSnapshot afterClear = Register(system, section, definitionId: 1);
  Require(
    afterClear.Handle.SlotGeneration == beforeClear.Handle.SlotGeneration + 1,
    "Clear must preserve slot generation history for stale-handle rejection.");
}

VerifyDefinitionCatalog();
VerifyRuntimeRootAndSlotReuse();
VerifySectionActivationAndCompaction();
VerifySectionHoleReuse();
VerifyBorrowedOperationsAreRejectedAtomically();
VerifyTargetAndMemberRemovalCleanBothDirections();
VerifyTileEntityAndLeashedShareOneRuntime();
VerifyNetworkValidationAndCommand();
VerifyClearPreservesGenerationHistory();
Console.WriteLine(
  "PASS: Leashed roots, generations, sections, anchor links and network validation core");
