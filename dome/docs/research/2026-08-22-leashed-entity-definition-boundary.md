# LeashedEntity Registration Boundary

Source oracle: `D:\TRbackup\Version4物理删除了某些文件\Terraria.GameContent\LeashedEntity.cs`

`Registry.RegisterAll` has a deterministic prototype order: a null sentinel at type `0`, then
`LeashedKite` at `1`, followed by eighteen critter prototypes through `WaterStrider` at `19`.
The registration sequence is source-observable and the V1456 compatibility parser already
rejects entity types outside `1..19`.

This candidate is **not accepted as an M-001 Simulation definition child**. The same source type
owns section streaming, active-section activation/deactivation, spawn/despawn, update, draw,
`NetModule.Sync`, and client section callbacks. None of those responsibilities has a unique
server Simulation owner or a persistence/replay contract in the current tree. Extracting only
names and IDs would create a registry with no authoritative consumer and would overstate coverage.

## Decision

Keep `LeashedEntity.Registry.RegisterAll` `unknown/deferred`. Retain the existing protocol-only
type bound and parser behavior. A future card needs a named server leash authority, stable entity
identity, section/PVS lifecycle, persistence, replay and protocol projection before any Simulation
definition is introduced.

This is a qualification rejection, not a claim that the source registration is absent. It also
does not change the existing M-001 `TileEntity` or `TorchID` child acceptances.
