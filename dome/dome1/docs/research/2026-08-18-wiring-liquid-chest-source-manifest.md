# Wiring、Liquid、Chest 事实源清单

状态：`frozen`（2026-08-18）

本清单把旧版源码当作只读事实源。任何迁移成员必须先在这里出现，再进入行为覆盖表；
清单本身不表示行为已经迁移。

## Legacy source

事实源目录：`D:\TRbackup\Version4物理删除了某些文件\Terraria`

| 文件 | 行数 | 字节数 | SHA-256 |
| --- | ---: | ---: | --- |
| `Wiring.cs` | 2,781 | 69,871 | `3D4D4C75B7A002207029294D63554B0BF376A29588AC7A06F62A08CC6A998225` |
| `Liquid.cs` | 1,550 | 37,986 | `23C27E5B669B99FE225ECFACEBD6F5254A2BA63239B6906CEF2C070010E709B6` |
| `Chest.cs` | 1,299 | 28,702 | `14EAF2C87C3761E2585C71EA98D6DF2F653354207BFE726D771E5C353F2918B9` |

## Member index

### Wiring

`SetCurrentUser`, `Initialize`, `SkipWire`, `ClearAll`, `UpdateMech`, `HitSwitch`,
`PokeLogicGate`, `Actuate`, `ActuateForced`, `MassWireOperation`, `TripWire`, `PixelBoxPass`,
`LogicGatePass`, `CheckLogicGate`, `HitWire`, `GetProjectileSource`, `GetNPCSource`,
`GetItemSource`, `HitWireSingle`, `Extractinator`, `IsHopperInRangeOf`, `Hopper`,
`ToggleHolidayLight`, `ToggleHangingLantern`, `Toggle2x2Light`, `ToggleLampPost`,
`ToggleTorch`, `ToggleCandle`, `ToggleLamp`, `ToggleChandelier`, `ToggleCampFire`,
`ToggleFirePlace`, `ExplodeMine`, `GeyserTrap`, `Teleport`, `TeleporterHitboxIntersects`,
`DeActive`, `ReActive`, `MassWireOperationInner`, `CheckMech`, `XferWater`.

### Liquid

`NetSendLiquid`, `tilesIgnoreWater`, `worldGenTilesIgnoreWater`, `ReInit`, `QuickWater`,
`SettleWaterAt`, `AttemptToMoveHoney`, `AttemptToMoveLava`, `AttemptToMoveShimmer`,
`UpdateProgressDisplay`, `Update`, `StartPanic`, `UpdateLiquid`, `AddWater`, `LiquidCheck`,
`UndergroundDesertCheck`, `CreateLiquidMergeTile`, `GetLiquidMergeTypes`, `LavaCheck`,
`HoneyCheck`, `ShimmerCheck`, `DelWater`.

### Chest

`Clear`, `CreateWorldChest`, `Assign`, `Resize`, `RemoveChest`, `CreateBank`, `CreateShop`,
`CreateOutOfArray`, `FillWithEmptyInstances`, `Initialize`, `GetCurrentlyOpenChests`, `IsLocked`,
`Unlock`, `Lock`, `UsingChest`, `FindChest`, `FindEmptyChest`, `NearOtherChests`,
`AfterPlacement_Hook`, `CreateChest`, `CanDestroyChest`, `DestroyChest`, `DestroyChestDirect`,
`SetupTravelShop_AddToShop`, `SetupTravelShop_CanAddItemToShop`, `SetupTravelShop_GetPainting`,
`SetupTravelShop_AdjustSlotRarities`, `SetupTravelShop_GetItem`, `SetupTravelShop`,
`SetupShop`, `GetNewParticle`, `UpdateChestFrames`, `FixLoadedData`, `ToString`.

## Current anchors

- `WorldGrid` owns dense tiles and section versions; tile mutations are committed through
  `TileChangeCommand`.
- `WorldTile` already carries liquid amount/type and frame values.
- `DomeSimulation` owns current chest compatibility state and persistence projection.
- Protocol V1456 has typed chest and liquid encoders plus source message cases 19, 31-34, 69,
  82, 108-110.

## Reproduction

```powershell
$source = 'D:\TRbackup\Version4物理删除了某些文件\Terraria'
foreach ($name in 'Wiring.cs', 'Liquid.cs', 'Chest.cs') {
  $path = Join-Path $source $name
  Get-FileHash $path -Algorithm SHA256
  (Get-Content $path).Count
  (Get-Item $path).Length
}
```

## 2026-08-19 Member Index Audit

The current source files reproduce the hashes above. A method-declaration comparison found `41`
Wiring methods, `22` Liquid methods, and `34` Chest methods. The member index lists all of them.
This is an inventory-only result: inclusion identifies a legacy behavior for coverage classification;
it does not change that behavior's migration status.
