# World-item stack merge boundary

## Source oracle

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\WorldItem.cs`
  - SHA-256: `A220C1C7AFDA67DDAFD296DA3E8EE14B8B8E1CC37492B01BF089080195D752CD`
  - `TryCombiningIntoNearbyItems`: lines `231-253`
- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Item.cs`
  - SHA-256: `932AA78145344BF9D825FB42ECD71A732BDDCE36474442D9CCB7B06A6F385E55`
  - `CanPassivelyStackInWorld`: lines `48652-48670`
  - `CanStack`: lines `48869-48879`

The legacy passive path combines only eligible items, requires matching stack identity/prefix,
owner reservation and shimmer state, uses a Manhattan distance threshold, updates position and
velocity, and turns the donor item to air when empty.

## Accepted narrow route

`WorldItemStackingSystem.TryMerge` owns a deterministic server merge child. It requires active
same-section items, compatible typed definitions and instance metadata, no pickup delay, a stable
replication-id order, a caller-supplied distance bound, and a same-tick merge guard. It enforces
the definition stack limit, increments both revisions and deactivates an emptied donor.

## Deferred

This is not full passive `TryCombiningIntoNearbyItems` parity. The current world-item model does
not yet carry source-equivalent owner reservation, shimmer state or passive-eligibility overrides;
its distance input is an explicit simulation bound rather than an imported 30-pixel Manhattan
rule. Position/velocity interpolation and network updates remain separate responsibilities.
