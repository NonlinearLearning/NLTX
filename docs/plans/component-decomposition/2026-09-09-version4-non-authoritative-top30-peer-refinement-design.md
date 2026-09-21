# Version4 Non-Authoritative Top-30 Peer Refinement Design

## Goal

Review the first 30 final fine subsystems in the non-authoritative member inventory and refine
only the groups with a confirmed declaration-type or source-path boundary. Preserve all 4,542
member facts and produce a traceable third-level peer mapping.

## Scope

This is a source-inventory navigation change. It updates the deterministic PowerShell generator,
its verifiers, the generated Markdown report, and this design record. It does not modify `src/`,
the Version4 reference checkout, the formal owner index, or runtime ECS code.

## Evidence And Decision

The current report has 4,017 fields, 525 properties, and 289 active final peer groups. The
following first-30 groups have stable declaration-type or source-file seams:

| Current peer | Evidence boundary | New peers | Counts |
|---|---|---|---|
| `UiItemSortingDefinitions` | `ItemSortingLayer`/`ItemSortingLayers` catalog types vs `ItemSorting` registry and `DamageTypeSortingLayerEntry` | `UiItemSortingLayerCatalogState`, `UiItemSortingRegistryAndRankingState` | 54 fields; 8 fields + 1 property |
| `MapTileEncodingAndStorageState` | `MapHelper` encoding/I/O state vs `MapTile` cell value state | `MapEncodingCatalogAndIoState`, `MapTileCellState` | 36 fields; 3 fields + 3 properties |
| `SharedContentUiTextAndOptionWidgets` | `GroupOptionButton<T>` vs `UIText`/`UIHeader` | `SharedContentUiOptionButtonState`, `SharedContentUiTextAndHeaderState` | 19 fields + 6 properties; 9 fields + 8 properties |
| `SharedDungeonRoomCoreAndSettings` | `DungeonRoom` lifecycle object vs room settings type family | `SharedDungeonRoomCoreState`, `SharedDungeonRoomSettingsState` | 5 fields + 2 properties; 25 fields |
| `SharedDungeonHallCoreAndSettings` | `DungeonHall` lifecycle object vs hall settings type family | `SharedDungeonHallCoreState`, `SharedDungeonHallSettingsState` | 9 fields + 1 property; 19 fields |

The five current peers are retired as active groups. Each new peer retains the original 239-group
baseline and records its immediate retired peer in the third-level lineage table. The active count
therefore becomes `289 - 5 + 10 = 294`; member, field, property, and parent totals do not change.

The source evidence is the Version4 member inventory and the read-only reference sources:

- `D:\TRbackup\Version4\Terraria.UI\ItemSorting.cs` separates layer definitions from the
  registry/ranking fields and `DamageTypeSortingLayerEntry`.
- `D:\TRbackup\Version4\Terraria.Map\MapHelper.cs` and `MapTile.cs` are separate owners;
  `MapTile` packs its cell flags and color into `_extraData`.
- `D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs`, `UIText.cs`,
  and `UIHeader.cs` are separate UI type families.
- `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoom.cs` and
  `DungeonRoomSettings.cs`, plus the corresponding Hall files, separate lifecycle objects from
  configuration/settings state.
- SS14 is used only as an ECS organization reference. The local tModLoader v2026.07 mirror is
  used only for public-type cross-checks; neither source establishes Version4 reader/writer,
  scheduling, persistence, or network ownership.

## Non-Splits

The other first-30 groups remain unchanged. `TileObjectData`, TimeLogger, SceneMetrics, drop
rules, fishing definitions, cage animation, audio constants, Main metadata/background, PopupText,
shader families, Tile placement modules, WorldItem payload, and EmergencyStacking lack an
additional independent owner/lifecycle seam in the current inventory. `RemoteClient` and `Netplay`
have plausible semantic subgroups, but their reader/writer evidence is not yet sufficient for a
safe source-inventory boundary; defer them rather than split by field-name ranges.

## Boundary And Data Flow

```text
input member inventory
        |
        v
base fine subsystem (239 historical groups)
        |
        v
second-level peer (current 289-group report)
        |
        v
third-level peer (10 new groups for 5 retired peers)
        |
        v
Markdown summary + per-member projection
```

The generator remains the single ownership function. It will add a third-level mapping after the
existing base and second-level mapping. The report will retain the existing baseline line for
backward traceability and add an immediate-peer line for the ten new groups. A new summary section
will list the five retired current peers and their ten replacements.

## Roles And Seams

The catalog, definition, and UI peers are read-only definition/projection seams. MapTile and the
dungeon core/settings peers are state or snapshot seams. Cross-peer interaction must be expressed
through explicit queries, commands, events, or adapters in any future ECS implementation; this
report does not claim that those runtime ports already exist.

## Verification

The full verifier must continue to prove row fidelity, sequence closure `1..4,542`, field/property
totals, parent totals, baseline rollups, ID-file exclusion, and 289-to-294 lineage. The focused
split verifier must assert ten third-level IDs, five retired immediate peers, two active children
per retired peer, and exactly 294 active groups. A separate read-only summary audit will recompute
the ranking and all ten new counts from the generated report.

