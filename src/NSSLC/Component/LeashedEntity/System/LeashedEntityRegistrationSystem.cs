using System.Collections.ObjectModel;
using EntityEcs;
using EntityEcs.Components;
using Terraria.Relationships;
using Terraria.WorldStorage;

namespace Terraria.LeashedEntity;

/// <summary>
/// Owns Leashed runtime roots, legacy slot projections, section membership and anchor links.
/// Mutating operations preflight affected runtime roots before committing. Commits invoke no
/// external callbacks and rely on EntityRuntime's owner-thread access contract to prevent an
/// intervening borrow or lifecycle change. Network, tile entity, NPC and projectile effects remain
/// outside this core system.
/// </summary>
public sealed class LeashedEntityRegistrationSystem
{
  private readonly EntityRuntime _runtime;
  private readonly LeashedDefinitionCatalog _definitions;
  private readonly LeashedSectionIndex _sections;
  private readonly Dictionary<EntityReference, LeashedEntityRegistrationSnapshot> _byEntity = new();
  private readonly Dictionary<EntityReference, HashSet<EntityReference>> _membersByAnchor = new();
  private readonly List<LeashedEntityRegistrationSnapshot?> _byLegacySlot = new();
  private readonly List<uint> _slotGenerations = new();

  public LeashedEntityRegistrationSystem(
    EntityRuntime runtime,
    LeashedDefinitionCatalog definitions,
    LeashedSectionIndex? sections = null)
  {
    _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
    _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
    _sections = sections ?? new LeashedSectionIndex();
  }

  public LeashedDefinitionQuery Definitions => new(_definitions);

  public LeashedSectionIndex Sections => _sections;

  public int ActiveCount => _byEntity.Count;

  public LeashedEntityRegistrationSnapshot Register(
    LeashedEntityRegistrationRequest request)
  {
    if (!_definitions.TryGet(request.DefinitionId, out _))
    {
      throw new ArgumentOutOfRangeException(
        nameof(request),
        request.DefinitionId,
        "The definition is not registered.");
    }

    int slot = FindReusableSlot();
    uint generation = NextSlotGeneration(slot);
    bool sectionExisted = _sections.TryGetSnapshot(
      request.Section,
      out LeashedSectionSnapshot sectionBefore);
    bool sectionActive = sectionExisted ? sectionBefore.IsActive : request.SectionActive;
    RuntimeEntityHandle runtimeHandle = _runtime.CreateEntity();
    bool sectionIndexed = false;
    int sectionSlot = -1;
    EntityReference entityReference = EntityReference.None;

    try
    {
      Attach(runtimeHandle, new LeashedEntityStateComponent(request.DefinitionId));
      Attach(runtimeHandle, new LeashedEntityLegacySlotComponent(slot, generation));
      Attach(
        runtimeHandle,
        new LeashedEntityLifecycleComponent(
          LeashedEntityLifecycle.Active,
          sectionActive,
          transitionSequence: 1));
      Attach(
        runtimeHandle,
        new LeashedEntitySectionMembershipComponent(
          request.Section,
          sectionSlot: -1,
          sectionActive,
          lastActivationTick: null));
      Attach(
        runtimeHandle,
        new LeashedEntityAnchorRelationComponent(
          anchorReference: null,
          anchorCoordinate: null,
          persistentAnchorId: null,
          relationRevision: null));

      if (!_runtime.TryPublishEntity(runtimeHandle) ||
          !_runtime.TryGetReference(
            runtimeHandle,
            EntityReferenceScope.Any,
            out entityReference))
      {
        throw new InvalidOperationException("The Leashed runtime root could not be published.");
      }

      LeashedEntityHandle handle = new(entityReference, slot, generation);
      if (!sectionExisted)
      {
        _sections.SetActive(request.Section, request.SectionActive);
      }

      sectionSlot = _sections.Add(request.Section, handle);
      sectionIndexed = true;
      if (!_runtime.TryReplace(
        runtimeHandle,
        new LeashedEntitySectionMembershipComponent(
          request.Section,
          sectionSlot,
          sectionActive,
          lastActivationTick: null)))
      {
        throw new InvalidOperationException(
          "The Leashed section component could not be committed.");
      }

      LeashedEntityRegistrationSnapshot snapshot = CaptureSnapshot(runtimeHandle, entityReference);
      EnsureSlotCapacity(slot);
      _byEntity.Add(entityReference, snapshot);
      _byLegacySlot[slot] = snapshot;
      return snapshot;
    }
    catch
    {
      if (sectionIndexed)
      {
        _sections.Remove(
          request.Section,
          new LeashedEntityHandle(entityReference, slot, generation),
          sectionSlot);
        CompactAndUpdate(request.Section);
      }

      if (slot < _byLegacySlot.Count)
      {
        _byLegacySlot[slot] = null;
        TrimTrailingSlots();
      }

      _byEntity.Remove(entityReference);
      RemoveCandidateRoot(runtimeHandle);
      throw;
    }
  }

  public bool TryGet(
    EntityReference runtimeEntityReference,
    out LeashedEntityRegistrationSnapshot snapshot)
  {
    if (_byEntity.TryGetValue(
          runtimeEntityReference,
          out LeashedEntityRegistrationSnapshot? indexed) &&
        TryCaptureIndexed(indexed, out snapshot))
    {
      Replace(snapshot);
      return true;
    }

    snapshot = null!;
    return false;
  }

  public bool TryGetByLegacySlot(
    int slot,
    uint generation,
    out LeashedEntityRegistrationSnapshot snapshot)
  {
    snapshot = null!;
    if (slot < 0 ||
        slot >= _byLegacySlot.Count ||
        _byLegacySlot[slot] is not { } candidate ||
        candidate.LegacySlot.SlotGeneration != generation ||
        !TryGet(candidate.RuntimeEntityReference, out snapshot))
    {
      snapshot = null!;
      return false;
    }

    return true;
  }

  public bool TryPeekByLegacySlot(
    int slot,
    out LeashedEntityRegistrationSnapshot snapshot)
  {
    snapshot = null!;
    if (slot < 0 ||
        slot >= _byLegacySlot.Count ||
        _byLegacySlot[slot] is not { } candidate ||
        !TryGet(candidate.RuntimeEntityReference, out snapshot))
    {
      snapshot = null!;
      return false;
    }

    return true;
  }

  public bool SetAnchor(
    LeashedEntityHandle handle,
    EntityReference? anchorReference,
    TileCoordinate? anchorCoordinate = null,
    TileEntityId? persistentAnchorId = null,
    long? relationRevision = null)
  {
    if (anchorReference is { } requestedAnchor &&
        (requestedAnchor.IsEmpty || requestedAnchor.Scope == EntityReferenceScope.None))
    {
      throw new ArgumentException(
        "An anchor relation requires a scoped entity reference.",
        nameof(anchorReference));
    }

    if (!TryGetByLegacySlot(
          handle.LegacySlot,
          handle.SlotGeneration,
          out LeashedEntityRegistrationSnapshot member) ||
        member.Handle != handle ||
        !_runtime.TryResolve(member.RuntimeEntityReference, out RuntimeEntityHandle memberHandle) ||
        !TryRead(memberHandle, out LeashedEntityAnchorRelationComponent current))
    {
      return false;
    }

    if (anchorReference is { } nextAnchor &&
        (!_runtime.TryResolve(nextAnchor, out _) || nextAnchor == member.RuntimeEntityReference))
    {
      return false;
    }

    if (current.AnchorReference is { } oldAnchor &&
        oldAnchor != anchorReference &&
        !CanRemoveAnchorLink(oldAnchor, member.RuntimeEntityReference))
    {
      return false;
    }

    if (anchorReference is { } newAnchor &&
        anchorReference != current.AnchorReference &&
        !CanAddAnchorLink(newAnchor))
    {
      return false;
    }

    LeashedEntityAnchorRelationComponent updated = new(
      anchorReference,
      anchorCoordinate,
      persistentAnchorId,
      relationRevision);

    if (current.AnchorReference == anchorReference)
    {
      if (anchorReference is { } unchangedAnchor &&
          !CanRemoveAnchorLink(unchangedAnchor, member.RuntimeEntityReference))
      {
        return false;
      }

      if (!_runtime.TryReplace(memberHandle, updated))
      {
        return false;
      }

      RefreshProjection(member.RuntimeEntityReference, memberHandle);
      return true;
    }

    if (anchorReference is { } anchorToAdd &&
        !AddAnchorLink(anchorToAdd, member.RuntimeEntityReference))
    {
      return false;
    }

    if (!_runtime.TryReplace(memberHandle, updated))
    {
      if (anchorReference is { } failedAnchor)
      {
        if (!RemoveAnchorLink(failedAnchor, member.RuntimeEntityReference))
        {
          throw new InvalidOperationException(
            "A rejected anchor change left its new reverse link attached.");
        }
      }

      return false;
    }

    if (current.AnchorReference is { } previousAnchor &&
        !RemoveAnchorLink(previousAnchor, member.RuntimeEntityReference))
    {
      if (!_runtime.TryReplace(memberHandle, current))
      {
        throw new InvalidOperationException(
          "Could not restore a rejected anchor change.");
      }

      if (anchorReference is { } rollbackAnchor)
      {
        if (!RemoveAnchorLink(rollbackAnchor, member.RuntimeEntityReference))
        {
          throw new InvalidOperationException(
            "A rejected anchor change left its new reverse link attached.");
        }
      }

      return false;
    }

    RefreshProjection(member.RuntimeEntityReference, memberHandle);
    return true;
  }

  /// <summary>
  /// Clears live incoming links before an anchor owner removes its runtime entity.
  /// The caller may invoke this immediately before or after the anchor root is removed.
  /// </summary>
  public int RemoveAnchorTarget(EntityReference targetReference)
  {
    if (!TryRemoveAnchorTarget(targetReference, out int cleared))
    {
      throw new InvalidOperationException("An affected Leashed root is currently borrowed.");
    }

    return cleared;
  }

  public bool TryRemoveAnchorTarget(EntityReference targetReference, out int cleared)
  {
    cleared = 0;
    if (targetReference.IsEmpty || targetReference.Scope == EntityReferenceScope.None)
    {
      throw new ArgumentException(
        "An anchor target reference is required.",
        nameof(targetReference));
    }

    HashSet<EntityReference> members = new();
    if (_membersByAnchor.TryGetValue(
          targetReference,
          out HashSet<EntityReference>? projectedMembers))
    {
      members.UnionWith(projectedMembers);
    }

    bool targetExists = _runtime.TryResolve(targetReference, out RuntimeEntityHandle targetHandle);
    bool targetHasLinks = targetExists && _runtime.Has<LeashedAnchorLinksComponent>(targetHandle);
    LeashedAnchorLinksComponent links = default;
    if (targetHasLinks)
    {
      if (!TryReadAnchorLinks(targetHandle, out links))
      {
        return false;
      }

      members.UnionWith(links.CopyMembers());
    }
    else if (targetExists && members.Count > 0)
    {
      return false;
    }

    List<AnchorMemberUpdate> updates = new(members.Count);
    foreach (EntityReference memberReference in members)
    {
      if (memberReference == targetReference ||
          !_runtime.TryResolve(memberReference, out RuntimeEntityHandle memberHandle))
      {
        continue;
      }

      if (!_runtime.Has<LeashedEntityAnchorRelationComponent>(memberHandle) ||
          !TryRead(memberHandle, out LeashedEntityAnchorRelationComponent relation))
      {
        return false;
      }

      if (relation.AnchorReference == targetReference)
      {
        updates.Add(new AnchorMemberUpdate(
          memberReference,
          memberHandle,
          relation,
          relation with { AnchorReference = null }));
      }
    }

    int committed = 0;
    foreach (AnchorMemberUpdate update in updates)
    {
      if (!_runtime.TryReplace(update.RuntimeHandle, update.UpdatedRelation))
      {
        RollbackAnchorMemberUpdates(updates, committed);
        return false;
      }

      committed++;
    }

    if (targetHasLinks && !_runtime.TryReplace(targetHandle, default(LeashedAnchorLinksComponent)))
    {
      RollbackAnchorMemberUpdates(updates, committed);
      return false;
    }

    _membersByAnchor.Remove(targetReference);
    foreach (AnchorMemberUpdate update in updates)
    {
      RefreshProjection(update.MemberReference, update.RuntimeHandle);
    }

    cleared = updates.Count;
    return true;
  }

  public int SetSectionActive(SectionCoordinate section, bool active)
  {
    if (!TrySetSectionActive(section, active, out int updated))
    {
      throw new InvalidOperationException("A section member is currently borrowed.");
    }

    return updated;
  }

  public bool TrySetSectionActive(SectionCoordinate section, bool active, out int updated)
  {
    updated = 0;
    IReadOnlyList<LeashedEntityHandle> handles = _sections.GetHandles(section);
    List<SectionUpdate> updates = new(handles.Count);
    foreach (LeashedEntityHandle handle in handles)
    {
      if (!TryGetIndexed(
        handle.RuntimeEntityReference,
        out LeashedEntityRegistrationSnapshot snapshot,
        out RuntimeEntityHandle runtimeHandle) ||
          snapshot.Handle != handle ||
          snapshot.SectionMembership.Section != section ||
          !_sections.Contains(section, handle, snapshot.SectionMembership.SectionSlot))
      {
        return false;
      }

      updates.Add(new SectionUpdate(
        snapshot.RuntimeEntityReference,
        runtimeHandle,
        snapshot.Lifecycle,
        snapshot.SectionMembership,
        snapshot.Lifecycle with
        {
          Spawned = active,
          TransitionSequence = snapshot.Lifecycle.TransitionSequence + 1
        },
        snapshot.SectionMembership with { IsSectionActive = active }));
    }

    int committed = 0;
    foreach (SectionUpdate update in updates)
    {
      if (!_runtime.TryEditPair<
        LeashedEntityLifecycleComponent,
        LeashedEntitySectionMembershipComponent>(
          update.RuntimeHandle,
          (
            ref LeashedEntityLifecycleComponent lifecycle,
            ref LeashedEntitySectionMembershipComponent membership) =>
          {
            lifecycle = update.UpdatedLifecycle;
            membership = update.UpdatedMembership;
          }))
      {
        for (int i = committed - 1; i >= 0; i--)
        {
          SectionUpdate rollback = updates[i];
          if (!_runtime.TryEditPair<
            LeashedEntityLifecycleComponent,
            LeashedEntitySectionMembershipComponent>(
              rollback.RuntimeHandle,
              (
                ref LeashedEntityLifecycleComponent lifecycle,
                ref LeashedEntitySectionMembershipComponent membership) =>
              {
                lifecycle = rollback.PreviousLifecycle;
                membership = rollback.PreviousMembership;
              }))
          {
            throw new InvalidOperationException(
              "A rejected section update could not be rolled back.");
          }
        }

        return false;
      }

      committed++;
    }

    _sections.SetActive(section, active);
    foreach (SectionUpdate update in updates)
    {
      RefreshProjection(update.RuntimeEntityReference, update.RuntimeHandle);
    }

    updated = committed;
    return true;
  }

  public bool Remove(LeashedEntityHandle handle)
  {
    if (!TryGetByLegacySlot(
          handle.LegacySlot,
          handle.SlotGeneration,
          out LeashedEntityRegistrationSnapshot snapshot) ||
        snapshot.Handle != handle)
    {
      return false;
    }

    EntityReference entityReference = snapshot.RuntimeEntityReference;
    if (!_sections.Contains(
      snapshot.SectionMembership.Section,
      handle,
      snapshot.SectionMembership.SectionSlot) ||
        !CanCompactSection(snapshot.SectionMembership.Section, handle) ||
        (snapshot.AnchorRelation.AnchorReference is { } targetReference &&
         !CanRemoveAnchorLink(targetReference, entityReference)))
    {
      return false;
    }

    if (!TryRemoveAnchorTarget(entityReference, out _))
    {
      return false;
    }

    if (snapshot.AnchorRelation.AnchorReference is { } outgoingAnchor &&
        !RemoveAnchorLink(outgoingAnchor, entityReference))
    {
      throw new InvalidOperationException("A preflighted anchor link could not be removed.");
    }

    if (!_sections.Remove(
      snapshot.SectionMembership.Section,
      handle,
      snapshot.SectionMembership.SectionSlot))
    {
      throw new InvalidOperationException("The section index lost a registered entity.");
    }

    _byEntity.Remove(entityReference);
    _byLegacySlot[handle.LegacySlot] = null;
    TrimTrailingSlots();
    CompactAndUpdate(snapshot.SectionMembership.Section);

    if (_runtime.TryResolve(entityReference, out RuntimeEntityHandle runtimeHandle))
    {
      if (!_runtime.TryBeginTermination(runtimeHandle) || !_runtime.TryRemoveEntity(runtimeHandle))
      {
        throw new InvalidOperationException("The Leashed runtime root could not be removed.");
      }
    }

    return true;
  }

  public int Clear()
  {
    if (!TryClear(out int removed))
    {
      throw new InvalidOperationException("An affected entity or anchor is currently borrowed.");
    }

    return removed;
  }

  public bool TryClear(out int removed)
  {
    removed = 0;
    LeashedEntityRegistrationSnapshot[] snapshots = _byEntity.Values.ToArray();
    foreach (LeashedEntityRegistrationSnapshot indexed in snapshots)
    {
      if (!TryCaptureIndexed(indexed, out LeashedEntityRegistrationSnapshot snapshot, out _) ||
          !_sections.Contains(
            snapshot.SectionMembership.Section,
            snapshot.Handle,
            snapshot.SectionMembership.SectionSlot) ||
          !CanCompactSection(snapshot.SectionMembership.Section, snapshot.Handle) ||
          (snapshot.AnchorRelation.AnchorReference is { } targetReference &&
           !CanRemoveAnchorLink(targetReference, snapshot.RuntimeEntityReference)))
      {
        return false;
      }
    }

    foreach (EntityReference targetReference in _membersByAnchor.Keys.ToArray())
    {
      if (_runtime.TryResolve(targetReference, out RuntimeEntityHandle targetHandle) &&
          _runtime.Has<LeashedAnchorLinksComponent>(targetHandle) &&
          !TryReadAnchorLinks(targetHandle, out _))
      {
        return false;
      }
    }

    LeashedEntityHandle[] handles = _byEntity.Values.Select(snapshot => snapshot.Handle).ToArray();
    foreach (LeashedEntityHandle handle in handles)
    {
      if (!Remove(handle))
      {
        throw new InvalidOperationException("A preflighted registration could not be cleared.");
      }

      removed++;
    }

    foreach (EntityReference targetReference in _membersByAnchor.Keys.ToArray())
    {
      if (!TryRemoveAnchorTarget(targetReference, out _))
      {
        throw new InvalidOperationException("A preflighted anchor link could not be cleared.");
      }
    }

    _membersByAnchor.Clear();
    _byEntity.Clear();
    _byLegacySlot.Clear();
    _sections.Clear();
    return true;
  }

  public IReadOnlyList<LeashedEntityRegistrationSnapshot> Snapshot()
  {
    List<LeashedEntityRegistrationSnapshot> snapshots = new(_byEntity.Count);
    foreach (EntityReference reference in _byEntity.Keys.ToArray())
    {
      if (TryGet(reference, out LeashedEntityRegistrationSnapshot snapshot))
      {
        snapshots.Add(snapshot);
      }
    }

    return new ReadOnlyCollection<LeashedEntityRegistrationSnapshot>(snapshots);
  }

  private bool AddAnchorLink(EntityReference targetReference, EntityReference memberReference)
  {
    if (!_runtime.TryResolve(targetReference, out RuntimeEntityHandle targetHandle))
    {
      return false;
    }

    bool hasLinks = TryReadAnchorLinks(targetHandle, out LeashedAnchorLinksComponent currentLinks);
    LeashedAnchorLinksComponent updatedLinks = currentLinks.WithAdded(memberReference);
    bool committed = hasLinks
      ? _runtime.TryReplace(targetHandle, updatedLinks)
      : _runtime.TryAttach(targetHandle, updatedLinks);
    if (!committed)
    {
      return false;
    }

    if (!_membersByAnchor.TryGetValue(targetReference, out HashSet<EntityReference>? members))
    {
      members = new HashSet<EntityReference>();
      _membersByAnchor.Add(targetReference, members);
    }

    members.Add(memberReference);
    return true;
  }

  private bool CanAddAnchorLink(EntityReference targetReference)
  {
    if (!_runtime.TryResolve(targetReference, out RuntimeEntityHandle targetHandle))
    {
      return false;
    }

    return TryRead(targetHandle, out EntityIdentityComponent _) &&
      (!_runtime.Has<LeashedAnchorLinksComponent>(targetHandle) ||
       TryReadAnchorLinks(targetHandle, out _));
  }

  private bool CanRemoveAnchorLink(EntityReference targetReference, EntityReference memberReference)
  {
    if (!_runtime.TryResolve(targetReference, out RuntimeEntityHandle targetHandle))
    {
      return true;
    }

    return _runtime.Has<LeashedAnchorLinksComponent>(targetHandle) &&
      TryReadAnchorLinks(targetHandle, out LeashedAnchorLinksComponent links) &&
      links.Contains(memberReference);
  }

  private bool RemoveAnchorLink(EntityReference targetReference, EntityReference memberReference)
  {
    if (_runtime.TryResolve(targetReference, out RuntimeEntityHandle targetHandle))
    {
      if (!_runtime.Has<LeashedAnchorLinksComponent>(targetHandle) ||
          !TryReadAnchorLinks(targetHandle, out LeashedAnchorLinksComponent links) ||
          !links.Contains(memberReference) ||
          !_runtime.TryReplace(targetHandle, links.WithRemoved(memberReference)))
      {
        return false;
      }
    }

    RemoveAnchorProjection(targetReference, memberReference);
    return true;
  }

  private void RemoveAnchorProjection(
    EntityReference targetReference,
    EntityReference memberReference)
  {
    if (!_membersByAnchor.TryGetValue(targetReference, out HashSet<EntityReference>? members))
    {
      return;
    }

    members.Remove(memberReference);
    if (members.Count == 0)
    {
      _membersByAnchor.Remove(targetReference);
    }
  }

  private bool CanCompactSection(SectionCoordinate section, LeashedEntityHandle removing)
  {
    if (!_sections.TryGetSnapshot(section, out LeashedSectionSnapshot snapshot))
    {
      return false;
    }

    int emptyAfterRemoval = snapshot.EmptySlots + 1;
    if (emptyAfterRemoval < snapshot.SlotCount / 2)
    {
      return true;
    }

    foreach (LeashedEntityHandle candidate in snapshot.Handles)
    {
      if (candidate == removing)
      {
        continue;
      }

      if (!TryGetIndexed(
            candidate.RuntimeEntityReference,
            out LeashedEntityRegistrationSnapshot member,
            out _) ||
          member.Handle != candidate)
      {
        return false;
      }
    }

    return true;
  }

  private void RollbackAnchorMemberUpdates(IReadOnlyList<AnchorMemberUpdate> updates, int committed)
  {
    for (int i = committed - 1; i >= 0; i--)
    {
      AnchorMemberUpdate rollback = updates[i];
      if (!_runtime.TryReplace(rollback.RuntimeHandle, rollback.PreviousRelation))
      {
        throw new InvalidOperationException("A rejected target removal could not be rolled back.");
      }
    }
  }

  private bool TryGetIndexed(
    EntityReference reference,
    out LeashedEntityRegistrationSnapshot snapshot,
    out RuntimeEntityHandle runtimeHandle)
  {
    snapshot = null!;
    runtimeHandle = default;
    return _byEntity.TryGetValue(reference, out LeashedEntityRegistrationSnapshot? candidate) &&
      TryCaptureIndexed(candidate, out snapshot, out runtimeHandle);
  }

  private bool TryCaptureIndexed(
    LeashedEntityRegistrationSnapshot indexed,
    out LeashedEntityRegistrationSnapshot snapshot)
  {
    return TryCaptureIndexed(indexed, out snapshot, out _);
  }

  private bool TryCaptureIndexed(
    LeashedEntityRegistrationSnapshot indexed,
    out LeashedEntityRegistrationSnapshot snapshot,
    out RuntimeEntityHandle runtimeHandle)
  {
    snapshot = null!;
    runtimeHandle = default;
    if (!_runtime.TryResolve(indexed.RuntimeEntityReference, out runtimeHandle) ||
        !TryRead(runtimeHandle, out LeashedEntityStateComponent state) ||
        !TryRead(runtimeHandle, out LeashedEntityLegacySlotComponent legacySlot) ||
        !TryRead(runtimeHandle, out LeashedEntityLifecycleComponent lifecycle) ||
        !TryRead(runtimeHandle, out LeashedEntitySectionMembershipComponent membership) ||
        !TryRead(runtimeHandle, out LeashedEntityAnchorRelationComponent anchorRelation) ||
        legacySlot.Slot != indexed.LegacySlot.Slot ||
        legacySlot.SlotGeneration != indexed.LegacySlot.SlotGeneration)
    {
      return false;
    }

    snapshot = new LeashedEntityRegistrationSnapshot(
      indexed.RuntimeEntityReference,
      state,
      legacySlot,
      lifecycle,
      membership,
      anchorRelation);
    return true;
  }

  private LeashedEntityRegistrationSnapshot CaptureSnapshot(
    RuntimeEntityHandle runtimeHandle,
    EntityReference entityReference)
  {
    if (!TryRead(runtimeHandle, out LeashedEntityStateComponent state) ||
        !TryRead(runtimeHandle, out LeashedEntityLegacySlotComponent legacySlot) ||
        !TryRead(runtimeHandle, out LeashedEntityLifecycleComponent lifecycle) ||
        !TryRead(runtimeHandle, out LeashedEntitySectionMembershipComponent membership) ||
        !TryRead(runtimeHandle, out LeashedEntityAnchorRelationComponent anchorRelation))
    {
      throw new InvalidOperationException("A required Leashed component is missing.");
    }

    return new LeashedEntityRegistrationSnapshot(
      entityReference,
      state,
      legacySlot,
      lifecycle,
      membership,
      anchorRelation);
  }

  private bool TryRead<TComponent>(RuntimeEntityHandle runtimeHandle, out TComponent component)
    where TComponent : struct
  {
    return _runtime.TryCapture<TComponent, TComponent>(
      runtimeHandle,
      static value => value,
      out component);
  }

  private bool TryReadAnchorLinks(
    RuntimeEntityHandle runtimeHandle,
    out LeashedAnchorLinksComponent links)
  {
    EntityReference[]? members = null;
    bool found = _runtime.TryInspect<LeashedAnchorLinksComponent>(
      runtimeHandle,
      (in LeashedAnchorLinksComponent component) => members = component.CopyMembers());
    links = found
      ? LeashedAnchorLinksComponent.FromMembers(members!)
      : default;
    return found;
  }

  private void RefreshProjection(EntityReference reference, RuntimeEntityHandle runtimeHandle)
  {
    if (!_byEntity.ContainsKey(reference))
    {
      return;
    }

    Replace(CaptureSnapshot(runtimeHandle, reference));
  }

  private void Replace(LeashedEntityRegistrationSnapshot snapshot)
  {
    _byEntity[snapshot.RuntimeEntityReference] = snapshot;
    if (snapshot.LegacySlot.Slot >= 0 && snapshot.LegacySlot.Slot < _byLegacySlot.Count)
    {
      _byLegacySlot[snapshot.LegacySlot.Slot] = snapshot;
    }
  }

  private void CompactAndUpdate(SectionCoordinate section)
  {
    IReadOnlyList<LeashedSectionSlotChange> changes = _sections.CompactIfNecessary(section);
    foreach (LeashedSectionSlotChange change in changes)
    {
      if (!TryGetIndexed(
            change.Handle.RuntimeEntityReference,
            out LeashedEntityRegistrationSnapshot moved,
            out RuntimeEntityHandle runtimeHandle) ||
          moved.Handle != change.Handle)
      {
        continue;
      }

      LeashedEntitySectionMembershipComponent membership = moved.SectionMembership with
      {
        SectionSlot = change.CurrentSlot
      };
      if (!_runtime.TryReplace(runtimeHandle, membership))
      {
        throw new InvalidOperationException("A compacted section slot could not be committed.");
      }

      RefreshProjection(moved.RuntimeEntityReference, runtimeHandle);
    }
  }

  private void Attach<TComponent>(RuntimeEntityHandle runtimeHandle, TComponent component)
    where TComponent : notnull
  {
    if (!_runtime.TryAttach(runtimeHandle, component))
    {
      throw new InvalidOperationException(
        $"The Leashed runtime component could not be attached: {typeof(TComponent).Name}");
    }
  }

  private int FindReusableSlot()
  {
    for (int i = 0; i < _byLegacySlot.Count; i++)
    {
      if (_byLegacySlot[i] is null)
      {
        return i;
      }
    }

    return _byLegacySlot.Count;
  }

  private uint NextSlotGeneration(int slot)
  {
    while (_slotGenerations.Count <= slot)
    {
      _slotGenerations.Add(0);
    }

    uint previous = _slotGenerations[slot];
    if (previous == uint.MaxValue)
    {
      throw new InvalidOperationException($"Legacy slot generation exhausted: {slot}");
    }

    uint next = previous + 1;
    _slotGenerations[slot] = next;
    return next;
  }

  private void EnsureSlotCapacity(int slot)
  {
    while (_byLegacySlot.Count <= slot)
    {
      _byLegacySlot.Add(null);
    }
  }

  private void TrimTrailingSlots()
  {
    while (_byLegacySlot.Count > 0 && _byLegacySlot[^1] is null)
    {
      _byLegacySlot.RemoveAt(_byLegacySlot.Count - 1);
    }
  }

  private void RemoveCandidateRoot(RuntimeEntityHandle runtimeHandle)
  {
    if (_runtime.TryGetStatus(runtimeHandle, out EntityRuntimeStatus status))
    {
      if (status == EntityRuntimeStatus.Running)
      {
        _runtime.TryBeginTermination(runtimeHandle);
      }

      _runtime.TryRemoveEntity(runtimeHandle);
    }
  }

  private readonly record struct AnchorMemberUpdate(
    EntityReference MemberReference,
    RuntimeEntityHandle RuntimeHandle,
    LeashedEntityAnchorRelationComponent PreviousRelation,
    LeashedEntityAnchorRelationComponent UpdatedRelation);

  private readonly record struct SectionUpdate(
    EntityReference RuntimeEntityReference,
    RuntimeEntityHandle RuntimeHandle,
    LeashedEntityLifecycleComponent PreviousLifecycle,
    LeashedEntitySectionMembershipComponent PreviousMembership,
    LeashedEntityLifecycleComponent UpdatedLifecycle,
    LeashedEntitySectionMembershipComponent UpdatedMembership);
}
