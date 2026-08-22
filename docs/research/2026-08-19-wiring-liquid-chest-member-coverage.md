# Wiring, Liquid, Chest Member Coverage Ledger

## Purpose

This ledger classifies every method in the frozen Version4 member index. A classification identifies
the current migration boundary; it does not claim replacement of the legacy class. `verified` is
limited to the cited executable slice, `partial` requires a later design/implementation decision,
and `excluded` records a deliberate first-batch boundary.

The source identity and member-count reproduction are in
`2026-08-18-wiring-liquid-chest-source-manifest.md` and
`Build/diagnostics/2026-08-19-wiring-liquid-chest-member-index-audit.md`.

## Wiring

| Legacy members | Status | Current boundary |
| --- | --- | --- |
| `SetCurrentUser`, `Initialize`, `ClearAll` | partial | Legacy static lifecycle/user context has no equivalent global mutable state; current Simulation owns command inputs and per-tick traversal state. |
| `SkipWire`, `HitWire`, `HitWireSingle`, `TripWire`, `MassWireOperation`, `MassWireOperationInner` | verified | `WireTraversalSystem`, `WiringInputValidationSystem`, deterministic order/budget/dedup proof in `Terraria.Dome.Wiring.Verification`. Mass edit actor/range semantics remain outside the verified single-color traversal slice. |
| `UpdateMech`, `CheckMech` | verified | `MechanismActivationSystem` has bounded activation/cooldown state; focused Wiring and loopback evidence cover the selected mechanisms. |
| `HitSwitch` | verified | Server-owned wiring input enters `DomeSimulation.AdvanceWiring`; invalid source/range/budget is rejected before mutation. |
| `PokeLogicGate`, `LogicGatePass`, `CheckLogicGate` | verified | `LogicGateEvaluationSystem` and Wiring/Liquid/Chest loopback cover deterministic rising-edge actuator output. |
| `Actuate`, `ActuateForced`, `DeActive`, `ReActive` | verified | `ActuatorCommandSystem` emits typed Tile commands committed by `TileChangeCommitSystem`. Legacy force/Tile frame nuances remain out of slice. |
| `XferWater` | verified | `PumpCommandSystem` emits `LiquidTransferCommand`; Liquid commit consumes it in the cross-domain loopback. |
| `PixelBoxPass` | excluded | PixelBox behavior is intentionally outside the first mechanism batch; no input is silently accepted as an implemented PixelBox action. |
| `GetProjectileSource`, `GetNPCSource`, `GetItemSource`, `ExplodeMine`, `GeyserTrap` | excluded | Projectile/Npc/item spawn and trap source semantics require separate combat/authority contracts. |
| `Extractinator`, `IsHopperInRangeOf`, `Hopper` | excluded | Item conversion and world-item routing are not part of the selected Wiring slice. |
| `ToggleHolidayLight`, `ToggleHangingLantern`, `Toggle2x2Light`, `ToggleLampPost`, `ToggleTorch`, `ToggleCandle`, `ToggleLamp`, `ToggleChandelier`, `ToggleCampFire`, `ToggleFirePlace` | partial | `LampComponent` verifies selected lamp Tile transitions; the complete legacy shape/frame/light family is not migrated. |
| `Teleport`, `TeleporterHitboxIntersects` | excluded | Teleporter transport requires an explicit entity-position authority and PVS policy. |

## Liquid

| Legacy members | Status | Current boundary |
| --- | --- | --- |
| `NetSendLiquid` | verified | `LiquidReplicationSystem` produces immutable snapshots and `NetLiquidModule` loopback proves visible-section projection. |
| `tilesIgnoreWater`, `worldGenTilesIgnoreWater` | partial | Ordinary base solidity/platform is source-derived; dynamic WorldGen and QuickWater overrides are not represented as mutable policy. |
| `ReInit` | partial | `LiquidWorldStateComponent` owns bounded configuration/mode, but full legacy static counter reset parity is not required by the selected tick slice. |
| `QuickWater`, `SettleWaterAt`, `StartPanic`, `UpdateLiquid` | partial | Ordinary runtime Panic scheduling is now verified through `LiquidPanicPolicy` and a bounded state transition; legacy QuickWater row processing, generation-time StartPanic side effects, dynamic solidity, player-count capacity changes, and full UpdateLiquid cleanup/network behavior remain outside the approved slice. |
| `UpdateProgressDisplay` | excluded | Legacy generation progress/UI output is not Simulation authority or a server protocol projection. |
| `Update` | verified | `LiquidPropagationSystem` provides the selected bounded deterministic source propagation and retry behavior. |
| `AttemptToMoveHoney`, `AttemptToMoveLava`, `AttemptToMoveShimmer` | verified | `LiquidRuleRegistry` and ordered merge matrix cover the four current types' selected propagation/merge outcome. Legacy delay/frame/environment effects remain partial. |
| `AddWater` | partial | Input queuing and global-table ordinary contact destruction are verified; `TileObjectData`, WorldGen, and environment side effects are not. |
| `LiquidCheck`, `LavaCheck`, `HoneyCheck`, `ShimmerCheck`, `CreateLiquidMergeTile`, `GetLiquidMergeTypes` | verified | `LiquidMergeSystem` and `LiquidRuleRegistry` cover all 12 ordered type-pair outcomes, consumed-liquid commit, and result Tile projection. |
| `UndergroundDesertCheck` | excluded | Underground-desert environmental rule inputs are not modeled by the selected Simulation definitions. |
| `DelWater` | verified | `LiquidUpdateQueueComponent` consumes sources and `LiquidSettleSystem` bounds requeue attempts. |

## Chest

| Legacy members | Status | Current boundary |
| --- | --- | --- |
| `Clear`, `Initialize`, `Assign`, `RemoveChest`, `CreateWorldChest`, `CreateOutOfArray`, `FillWithEmptyInstances`, `Resize` | partial | `ChestMutationCommitSystem` and persistence own fixed 40-slot world Chest lifecycle; arbitrary legacy arrays/resizing are not modeled. |
| `GetCurrentlyOpenChests`, `UsingChest`, `FindChest`, `FindEmptyChest`, `NearOtherChests` | verified | `ChestIndexSystem`, `ChestOpenSystem`, exclusive opener and coordinate uniqueness are covered by WorldObjects/persistence/loopback verifiers. |
| `IsLocked`, `Unlock`, `Lock` | excluded | Locked Chest/key consumption and Tile frame behavior are not part of the selected world-Chest contract. |
| `AfterPlacement_Hook`, `CreateChest`, `CanDestroyChest`, `DestroyChest`, `DestroyChestDirect`, `UpdateChestFrames` | partial | Coordinate-indexed create/destroy of closed empty Chests is verified; object placement hooks, Tile frame integration and full legacy destruction effects remain unimplemented. |
| `CreateBank`, `CreateShop`, `SetupShop` | excluded | Bank ownership and shop inventory semantics are explicitly outside the first world-Chest batch. |
| `SetupTravelShop_AddToShop`, `SetupTravelShop_CanAddItemToShop`, `SetupTravelShop_GetPainting`, `SetupTravelShop_AdjustSlotRarities`, `SetupTravelShop_GetItem`, `SetupTravelShop` | excluded | Travel-shop random selection, player luck, and economy policy require a distinct definition/authority design. |
| `FixLoadedData` | partial | Candidate-load duplicate-coordinate rejection and snapshot restore are verified; full legacy item-instance repair is not migrated. |
| `GetNewParticle`, `ToString` | excluded | Client particle/UI formatting behavior is not a Simulation responsibility. |

## Gate Result

All 97 indexed members now have a written state and boundary. The ledger deliberately contains
`partial` rows, so it proves traceability rather than proposal completion. The proposal remains
`分阶段迁移中` until those behavior families are either implemented with evidence or explicitly
excluded through approved scope decisions.
