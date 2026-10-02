using System.Collections.ObjectModel;
using EntityEcs.Components;
using Terraria.WorldStorage;
namespace Terraria.LeashedEntity;

/// <summary>
/// Owns registration, legacy slot allocation and lifecycle commits for leashed entities.
/// Network, tile entity, NPC and projectile effects remain outside this core system.
/// </summary>
public sealed class LeashedEntityRegistrationSystem
{
  private readonly LeashedDefinitionCatalog _definitions;
  private readonly LeashedSectionIndex _sections;
  private readonly Dictionary<EntityId, LeashedEntityRegistrationSnapshot> _byEntity = new();
  private readonly List<LeashedEntityRegistrationSnapshot?> _byLegacySlot = new();
  private readonly List<uint> _slotGenerations = new();

  public LeashedEntityRegistrationSystem(
    LeashedDefinitionCatalog definitions,
    LeashedSectionIndex? sections = null)
  {
    _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
    _sections = sections ?? new LeashedSectionIndex();
  }

  public LeashedDefinitionQuery Definitions => new(_definitions);

  public LeashedSectionIndex Sections => _sections;

  public int ActiveCount => _byEntity.Count;

  public LeashedEntityRegistrationSnapshot Register(
    LeashedEntityRegistrationRequest request)
  {
    if (!request.RuntimeEntityId.IsAssigned)
    {
      throw new ArgumentException(
        "A runtime entity identity is required.",
        nameof(request));
    }

    if (_byEntity.ContainsKey(request.RuntimeEntityId))
    {
      throw new InvalidOperationException(
        $"The runtime entity is already registered: {request.RuntimeEntityId}");
    }

    if (!_definitions.TryGet(request.DefinitionId, out _))
    {
      throw new ArgumentOutOfRangeException(
        nameof(request),
        request.DefinitionId,
        "The definition is not registered.");
    }

    int slot = FindReusableSlot();
    uint generation = NextSlotGeneration(slot);
    LeashedEntityHandle handle =
      new(request.RuntimeEntityId, slot, generation);

    bool sectionActive;
    if (_sections.TryGetSnapshot(request.Section, out LeashedSectionSnapshot sectionSnapshot))
    {
      sectionActive = sectionSnapshot.IsActive;
    }
    else
    {
      _sections.SetActive(request.Section, request.SectionActive);
      sectionActive = request.SectionActive;
    }
    int sectionSlot = _sections.Add(request.Section, handle);

    LeashedEntityRegistrationSnapshot snapshot =
      new(
        request.RuntimeEntityId,
        new LeashedEntityStateComponent(request.DefinitionId),
        new LeashedEntityLegacySlotComponent(slot, generation),
        new LeashedEntityLifecycleComponent(
          LeashedEntityLifecycle.Active,
          sectionActive,
          transitionSequence: 1),
        new LeashedEntitySectionMembershipComponent(
          request.Section,
          sectionSlot,
          sectionActive,
          lastActivationTick: null));

    EnsureSlotCapacity(slot);
    _byLegacySlot[slot] = snapshot;
    _byEntity.Add(request.RuntimeEntityId, snapshot);
    return snapshot;
  }

  public bool TryGet(
    EntityId runtimeEntityId,
    out LeashedEntityRegistrationSnapshot snapshot)
  {
    return _byEntity.TryGetValue(runtimeEntityId, out snapshot!);
  }

  public bool TryGetByLegacySlot(
    int slot,
    uint generation,
    out LeashedEntityRegistrationSnapshot snapshot)
  {
    snapshot = null!;
    if (slot < 0
      || slot >= _byLegacySlot.Count
      || _byLegacySlot[slot] is not { } candidate
      || candidate.LegacySlot.SlotGeneration != generation)
    {
      return false;
    }

    snapshot = candidate;
    return true;
  }

  public bool TryPeekByLegacySlot(
    int slot,
    out LeashedEntityRegistrationSnapshot snapshot)
  {
    snapshot = null!;
    if (slot < 0 || slot >= _byLegacySlot.Count || _byLegacySlot[slot] is not { } candidate)
    {
      return false;
    }

    snapshot = candidate;
    return true;
  }

  public int SetSectionActive(SectionCoordinate section, bool active)
  {
    _sections.SetActive(section, active);
    IReadOnlyList<LeashedEntityHandle> handles = _sections.GetHandles(section);
    int updated = 0;
    foreach (LeashedEntityHandle handle in handles)
    {
      if (!_byEntity.TryGetValue(handle.RuntimeEntityId, out LeashedEntityRegistrationSnapshot? snapshot)
        || snapshot.Handle != handle)
      {
        continue;
      }

      LeashedEntityLifecycleComponent lifecycle =
        snapshot.Lifecycle with
        {
          Spawned = active,
          TransitionSequence = snapshot.Lifecycle.TransitionSequence + 1
        };
      LeashedEntitySectionMembershipComponent membership =
        snapshot.SectionMembership with
        {
          IsSectionActive = active
        };
      Replace(snapshot with { Lifecycle = lifecycle, SectionMembership = membership });
      updated++;
    }

    return updated;
  }

  public bool Remove(LeashedEntityHandle handle)
  {
    if (!TryGetByLegacySlot(handle.LegacySlot, handle.SlotGeneration, out LeashedEntityRegistrationSnapshot snapshot)
      || snapshot.Handle != handle)
    {
      return false;
    }

    if (!_sections.Remove(
      snapshot.SectionMembership.Section,
      handle,
      snapshot.SectionMembership.SectionSlot))
    {
      throw new InvalidOperationException(
        "The section index did not contain the registered entity.");
    }

    _byEntity.Remove(snapshot.RuntimeEntityId);
    _byLegacySlot[handle.LegacySlot] = null;
    TrimTrailingSlots();

    IReadOnlyList<LeashedSectionSlotChange> changes =
      _sections.CompactIfNecessary(snapshot.SectionMembership.Section);
    foreach (LeashedSectionSlotChange change in changes)
    {
      if (_byEntity.TryGetValue(
        change.Handle.RuntimeEntityId,
        out LeashedEntityRegistrationSnapshot? moved))
      {
        LeashedEntitySectionMembershipComponent membership =
          moved.SectionMembership with
          {
            SectionSlot = change.CurrentSlot
          };
        Replace(moved with { SectionMembership = membership });
      }
    }

    return true;
  }

  public int Clear()
  {
    int removed = _byEntity.Count;
    _byEntity.Clear();
    _byLegacySlot.Clear();
    _sections.Clear();
    return removed;
  }

  public IReadOnlyList<LeashedEntityRegistrationSnapshot> Snapshot()
  {
    return new ReadOnlyCollection<LeashedEntityRegistrationSnapshot>(
      _byEntity.Values.ToArray());
  }

  private void Replace(LeashedEntityRegistrationSnapshot snapshot)
  {
    _byEntity[snapshot.RuntimeEntityId] = snapshot;
    if (snapshot.LegacySlot.Slot >= 0
      && snapshot.LegacySlot.Slot < _byLegacySlot.Count)
    {
      _byLegacySlot[snapshot.LegacySlot.Slot] = snapshot;
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

    int slot = _byLegacySlot.Count;
    _byLegacySlot.Add(null);
    if (_slotGenerations.Count <= slot)
    {
      _slotGenerations.Add(0);
    }

    return slot;
  }

  private uint NextSlotGeneration(int slot)
  {
    uint previous = _slotGenerations[slot];
    if (previous == uint.MaxValue)
    {
      throw new InvalidOperationException(
        $"Legacy slot generation exhausted: {slot}");
    }

    uint next = previous + 1;
    _slotGenerations[slot] = next;
    return next;
  }

  private void EnsureSlotCapacity(int slot)
  {
    while (_slotGenerations.Count <= slot)
    {
      _slotGenerations.Add(0);
    }
  }

  private void TrimTrailingSlots()
  {
    while (_byLegacySlot.Count > 0
      && _byLegacySlot[^1] is null)
    {
      _byLegacySlot.RemoveAt(_byLegacySlot.Count - 1);
    }
  }
}
