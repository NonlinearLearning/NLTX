using EntityEcs.Components;
using Terraria.LeashedEntity;
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
  Require(query.TryGetByKey("walker", out LeashedDefinitionDescriptor resolved), "The key query must resolve.");
  Require(resolved.DefinitionId == walker.DefinitionId, "The key query must return the registered definition.");
  Require(query.TryGetById(kite.DefinitionId, out _), "The id query must resolve.");
  Require(!query.TryGetById(0, out _), "Definition id zero is the unbound sentinel.");

  catalog.Freeze();
  RequireThrows<InvalidOperationException>(
    () => catalog.Register("late", LeashedDefinitionKind.Critter),
    "A frozen catalog must reject late registration.");
}

static void VerifyRegistrationAndSlotReuse()
{
  LeashedDefinitionCatalog catalog = CreateCatalog();
  LeashedEntityRegistrationSystem system = new(catalog);
  SectionCoordinate section = new(2, 3);
  EntityId firstId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));
  EntityId secondId = new(Guid.Parse("00000000-0000-0000-0000-000000000002"));

  LeashedEntityRegistrationSnapshot first =
    system.Register(
      new LeashedEntityRegistrationRequest(
        firstId,
        2,
        section,
        SectionActive: true));

  Require(first.Handle.LegacySlot == 0, "The first entity must receive slot zero.");
  Require(first.Handle.SlotGeneration == 1, "The first slot generation must be one.");
  Require(first.Lifecycle.IsActive && first.Lifecycle.Spawned, "An active section must spawn the entity.");
  Require(system.TryGetByLegacySlot(0, 1, out _), "The assigned slot must be queryable.");

  Require(system.Remove(first.Handle), "The registered entity must be removable.");
  Require(!system.TryGetByLegacySlot(0, 1, out _), "A removed slot generation must be stale.");

  LeashedEntityRegistrationSnapshot replacement =
    system.Register(
      new LeashedEntityRegistrationRequest(
        secondId,
        2,
        section,
        SectionActive: true));

  Require(replacement.Handle.LegacySlot == 0, "A removed slot must be reusable.");
  Require(replacement.Handle.SlotGeneration == 2, "Slot reuse must increment the generation.");
  Require(!system.TryGet(firstId, out _), "The removed runtime identity must not remain queryable.");
}

static void VerifySectionActivationAndCompaction()
{
  LeashedDefinitionCatalog catalog = CreateCatalog();
  LeashedEntityRegistrationSystem system = new(catalog);
  SectionCoordinate section = new(5, 7);
  EntityId firstId = new(Guid.Parse("00000000-0000-0000-0000-000000000011"));
  EntityId secondId = new(Guid.Parse("00000000-0000-0000-0000-000000000012"));
  EntityId thirdId = new(Guid.Parse("00000000-0000-0000-0000-000000000013"));
  EntityId fourthId = new(Guid.Parse("00000000-0000-0000-0000-000000000014"));

  LeashedEntityRegistrationSnapshot first =
    system.Register(new(firstId, 2, section, SectionActive: true));
  LeashedEntityRegistrationSnapshot second =
    system.Register(new(secondId, 2, section, SectionActive: false));
  LeashedEntityRegistrationSnapshot third =
    system.Register(new(thirdId, 2, section, SectionActive: false));
  LeashedEntityRegistrationSnapshot fourth =
    system.Register(new(fourthId, 2, section, SectionActive: false));

  Require(second.Lifecycle.Spawned, "A new member must observe existing section activity.");
  Require(third.Lifecycle.Spawned, "A later new member must observe existing section activity.");
  Require(system.SetSectionActive(section, true) == 4, "Section activation must update all members.");
  Require(system.TryGet(thirdId, out LeashedEntityRegistrationSnapshot activeThird)
    && activeThird.Lifecycle.Spawned, "Section activation must mark members spawned.");

  Require(system.Remove(second.Handle), "The middle member must be removable.");
  Require(system.Sections.TryGetSnapshot(section, out LeashedSectionSnapshot withHole)
    && withHole.EmptySlots == 1,
    "A single removal must preserve a reusable hole before compaction.");
  Require(system.Remove(third.Handle), "A second member must be removable.");
  Require(system.Sections.TryGetSnapshot(section, out LeashedSectionSnapshot compacted), "The section snapshot must exist.");
  Require(compacted.ActiveCount == 2 && compacted.EmptySlots == 0, "Removal must compact a half-empty section.");
  Require(system.TryGet(fourthId, out LeashedEntityRegistrationSnapshot moved)
    && moved.SectionMembership.SectionSlot == 1,
    "Compaction must update the moved entity's section slot.");

  Require(system.SetSectionActive(section, false) == 2, "Section deactivation must update all members.");
  Require(system.TryGet(firstId, out LeashedEntityRegistrationSnapshot inactiveFirst)
    && !inactiveFirst.Lifecycle.Spawned,
    "Section deactivation must stop spawning without removing the entity.");
}

static void VerifySectionHoleReuse()
{
  LeashedDefinitionCatalog catalog = CreateCatalog();
  LeashedEntityRegistrationSystem system = new(catalog);
  SectionCoordinate section = new(8, 9);
  EntityId firstId = new(Guid.Parse("00000000-0000-0000-0000-000000000031"));
  EntityId secondId = new(Guid.Parse("00000000-0000-0000-0000-000000000032"));
  EntityId thirdId = new(Guid.Parse("00000000-0000-0000-0000-000000000033"));
  EntityId fourthId = new(Guid.Parse("00000000-0000-0000-0000-000000000034"));
  EntityId replacementId = new(Guid.Parse("00000000-0000-0000-0000-000000000035"));

  LeashedEntityRegistrationSnapshot first =
    system.Register(new(firstId, 2, section, SectionActive: true));
  LeashedEntityRegistrationSnapshot second =
    system.Register(new(secondId, 2, section, SectionActive: true));
  system.Register(new(thirdId, 2, section, SectionActive: true));
  system.Register(new(fourthId, 2, section, SectionActive: true));

  Require(system.Remove(first.Handle), "The first member must be removable without forced compaction.");
  Require(system.Sections.TryGetSnapshot(section, out LeashedSectionSnapshot beforeReuse)
    && beforeReuse.EmptySlots == 1
    && beforeReuse.SlotCount == 4,
    "The removed section slot must remain available for reuse.");

  LeashedEntityRegistrationSnapshot replacement =
    system.Register(new(replacementId, 2, section, SectionActive: true));
  Require(replacement.SectionMembership.SectionSlot == first.SectionMembership.SectionSlot,
    "A new member must reuse the removed section slot.");
  Require(system.Sections.TryGetSnapshot(section, out LeashedSectionSnapshot afterReuse)
    && afterReuse.EmptySlots == 0
    && afterReuse.SlotCount == 4,
    "Reusing a section slot must not grow the logical bucket.");
  Require(system.TryGet(secondId, out LeashedEntityRegistrationSnapshot retained)
    && retained.SectionMembership.SectionSlot == second.SectionMembership.SectionSlot,
    "Reusing a hole must preserve existing member slots.");
}

static void VerifyNetworkValidationAndCommand()
{
  LeashedDefinitionCatalog catalog = CreateCatalog();
  LeashedEntityRegistrationSystem system = new(catalog);
  LeashedEntityNetworkAdapter adapter = new();
  SectionCoordinate section = new(4, 6);
  EntityId entityId = new(Guid.Parse("00000000-0000-0000-0000-000000000041"));
  LeashedEntityRegistrationSnapshot current =
    system.Register(new(entityId, 2, section, SectionActive: true));

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
    new(LeashedNetworkFrameKind.PartialSync, current.Handle.LegacySlot, current.Handle.SlotGeneration + 1, 2, section);
  Require(adapter.Validate(staleGeneration, catalog, system).Status
    == LeashedNetworkFrameValidationStatus.RejectedStaleGeneration,
    "A partial sync with a stale generation must be rejected.");

  LeashedNetworkFrame typeMismatch =
    new(LeashedNetworkFrameKind.PartialSync, current.Handle.LegacySlot, current.Handle.SlotGeneration, 1, section);
  Require(adapter.Validate(typeMismatch, catalog, system).Status
    == LeashedNetworkFrameValidationStatus.RejectedTypeMismatch,
    "A partial sync with a different definition must be rejected.");

  LeashedNetworkFrame sectionMismatch =
    new(LeashedNetworkFrameKind.FullSync, current.Handle.LegacySlot, current.Handle.SlotGeneration, 2, new(4, 7));
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
    new(LeashedNetworkFrameKind.Remove, current.Handle.LegacySlot, current.Handle.SlotGeneration, 0, section);
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

static void VerifyClear()
{
  LeashedDefinitionCatalog catalog = CreateCatalog();
  LeashedEntityRegistrationSystem system = new(catalog);
  SectionCoordinate section = new(1, 1);
  EntityId firstId = new(Guid.Parse("00000000-0000-0000-0000-000000000021"));

  LeashedEntityRegistrationSnapshot beforeClear =
    system.Register(new(firstId, 1, section, SectionActive: true));
  Require(system.Clear() == 1, "Clear must report the number of active entities.");
  Require(system.ActiveCount == 0, "Clear must remove all runtime registrations.");
  Require(!system.TryGet(firstId, out _), "Cleared runtime identities must not resolve.");
  Require(!system.Sections.TryGetSnapshot(section, out _), "Clear must remove rebuildable section indexes.");

  LeashedEntityRegistrationSnapshot afterClear =
    system.Register(new(firstId, 1, section, SectionActive: true));
  Require(
    afterClear.Handle.SlotGeneration == beforeClear.Handle.SlotGeneration + 1,
    "Clear must preserve slot generation history for stale-handle rejection.");
}

VerifyDefinitionCatalog();
VerifyRegistrationAndSlotReuse();
VerifySectionActivationAndCompaction();
VerifySectionHoleReuse();
VerifyNetworkValidationAndCommand();
VerifyClear();
Console.WriteLine("PASS: P02 leashed entity catalog, slot lifecycle, section activation, compaction and network validation core");
