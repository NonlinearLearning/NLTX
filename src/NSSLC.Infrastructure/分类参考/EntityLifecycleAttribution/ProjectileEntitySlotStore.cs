namespace Terraria.EntityLifecycleAttribution;

public sealed class ProjectileEntitySlotStore
{
  private readonly Slot[] _slots;
  private readonly ProjectileIdentityIndex _identityIndex;
  private int _nextRuntimeEntityId;

  public ProjectileEntitySlotStore(int capacity, ProjectileIdentityIndex? identityIndex = null)
  {
    if (capacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    _slots = Enumerable.Range(0, capacity).Select(_ => new Slot()).ToArray();
    _identityIndex = identityIndex ?? new ProjectileIdentityIndex();
  }

  public int Capacity => _slots.Length;

  public bool TryAllocate(
    int owner,
    int identity,
    int projectileType,
    out EntitySlotHandle handle)
  {
    return TryAllocate(_nextRuntimeEntityId++, owner, identity, projectileType, out handle);
  }

  public bool TryAllocate(
    int runtimeEntityId,
    int owner,
    int identity,
    int projectileType,
    out EntitySlotHandle handle)
  {
    handle = default;
    if (runtimeEntityId < 0 || owner < 0 || identity < 0 || projectileType < 0 ||
        _identityIndex.TryResolve(owner, identity, out _))
    {
      return false;
    }

    for (int slotIndex = 0; slotIndex < _slots.Length; slotIndex++)
    {
      Slot slot = _slots[slotIndex];
      if (slot.Active)
      {
        continue;
      }

      slot.Active = true;
      slot.RuntimeEntityId = runtimeEntityId;
      slot.Owner = owner;
      slot.Identity = identity;
      slot.ProjectileType = projectileType;
      handle = EntitySlotHandle.ForPool(
        EntitySlotPoolKind.Projectile,
        slot.RuntimeEntityId,
        slotIndex,
        slot.Generation);
      if (!_identityIndex.TryRegister(owner, identity, handle))
      {
        slot.Active = false;
        slot.RuntimeEntityId = -1;
        return false;
      }

      return true;
    }

    return false;
  }

  public bool TryResolve(int owner, int identity, out EntitySlotHandle handle)
  {
    handle = default;
    if (!_identityIndex.TryResolve(owner, identity, out EntitySlotHandle indexedHandle))
    {
      return false;
    }

    if (indexedHandle.CompatibilitySlot < 0 || indexedHandle.CompatibilitySlot >= _slots.Length)
    {
      return false;
    }

    Slot slot = _slots[indexedHandle.CompatibilitySlot];
    if (!slot.Active || slot.Owner != owner || slot.Identity != identity ||
        indexedHandle.RuntimeEntityId != slot.RuntimeEntityId ||
        indexedHandle.Generation != slot.Generation)
    {
      _identityIndex.Remove(owner, identity, indexedHandle);
      return false;
    }

    handle = indexedHandle;
    return true;
  }

  public bool TryRelease(EntitySlotHandle handle)
  {
    if (!handle.MatchesPool(EntitySlotPoolKind.Projectile) ||
        handle.CompatibilitySlot < 0 ||
        handle.CompatibilitySlot >= _slots.Length)
    {
      return false;
    }

    Slot slot = _slots[handle.CompatibilitySlot];
    if (!slot.Active || slot.RuntimeEntityId != handle.RuntimeEntityId || slot.Generation != handle.Generation)
    {
      return false;
    }

    _identityIndex.Remove(slot.Owner, slot.Identity, handle);
    slot.Active = false;
    slot.RuntimeEntityId = -1;
    slot.Owner = -1;
    slot.Identity = -1;
    slot.ProjectileType = -1;
    AdvanceGeneration(slot);
    return true;
  }

  private static void AdvanceGeneration(Slot slot)
  {
    if (slot.Generation == int.MaxValue)
    {
      throw new InvalidOperationException("The projectile slot generation exhausted its range.");
    }

    slot.Generation++;
  }

  private sealed class Slot
  {
    public bool Active { get; set; }

    public int Generation { get; set; } = 1;

    public int RuntimeEntityId { get; set; } = -1;

    public int Owner { get; set; } = -1;

    public int Identity { get; set; } = -1;

    public int ProjectileType { get; set; } = -1;
  }
}
