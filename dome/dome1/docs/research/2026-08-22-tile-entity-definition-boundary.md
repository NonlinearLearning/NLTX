# TileEntity Definition Boundary

Source oracles:

- `D:\TRbackup\Version4物理删除了某些文件\Terraria.DataStructures\TileEntity.cs`
  - SHA-256: `8E164326E4876888D111A0CDD125CD846F222D28BA3C65940B5269FFB51B8E1B`
  - `InitializeAll`: `129-134`
- `D:\TRbackup\Version4物理删除了某些文件\Terraria.DataStructures\TileEntitiesManager.cs`
  - SHA-256: `784156FBB35011C339E678C63468B44ACE617FB58BA6955424C1B3D03772660D`
  - `RegisterAll`: `30-45`

## Accepted child scope

The source manager registers eleven prototypes in deterministic order. The new immutable
`TileEntityDefinitionRegistry` preserves the resulting type IDs `0..10` and names:

`TrainingDummy`, `ItemFrame`, `LogicSensor`, `DisplayDoll`, `WeaponsRack`, `HatRack`,
`FoodPlatter`, `TeleportationPylon`, `DeadCellsDisplayJar`, `KiteAnchor`, `CritterAnchor`.

Unknown type `11` is rejected. This is a fixed-definition registration boundary under
`Terraria.Dome.Simulation.WorldObjects.Definitions`.

## Explicitly deferred

The registration child does not claim payload decoding, placement validation, update scheduling,
item/equipment behavior, pylon travel, leash behavior, tile framing, network message parity or
complete tile-entity persistence semantics. Existing opaque compatibility records remain
fail-closed for unsupported payloads. The aggregate `TileEntity.InitializeAll` replacement is not
introduced.

## Verification

- `Test/Terraria.Dome.WorldGeneration.Verification --reduced`: exit `0`, 32 of 81 sections
  executed (39.5%); source order and unknown type rejection passed.
- `MainBoundary`: exit `0`, 771 Simulation source files, 0 violations.
- Scoped `git diff --check`: exit `0`.
