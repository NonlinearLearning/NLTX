# Item, Container, And Commerce Components Implementation Plan

**Goal:** Implement the component and value-type declarations defined by
`docs/Version4物品容器与经济事务组件设计.md` in `src/Items`.

**Architecture:** Keep core item and container state in the `Terraria.Items` namespace. Place the
independent economy and loot capabilities in `Terraria.Items.Commerce` and `Terraria.Items.Loot`.
Preserve the existing seven root components as compatibility types while adding the decomposed
state components; do not add systems, commands, adapters, or projections.

**Tech Stack:** C# preview, .NET 10, `Terraria.Relationships.EntityReference`.

---

### Task 1: Add shared item and world value types

**Files:**
- Create: `src/Items/TileCoordinates.cs`
- Create: `src/Items/WorldPosition.cs`
- Create: `src/Items/WorldVector.cs`
- Create: `src/Items/ItemUsePhase.cs`
- Create: `src/Items/ContainerKind.cs`

Define immutable, dependency-free value types and enums required by components. They own no
world lookup, clock, or rendering behavior.

### Task 2: Add decomposed item and container state components

**Files:**
- Create: `src/Items/ItemInstanceComponent.cs`
- Create: `src/Items/ItemUseComponent.cs`
- Create: `src/Items/CraftingComponent.cs`
- Create: `src/Items/CraftingMaterialReservation.cs`
- Create: `src/Items/ContainerCapacityComponent.cs`
- Create: `src/Items/ContainerContentsComponent.cs`
- Create: `src/Items/ContainerAccessComponent.cs`
- Create: `src/Items/WorldItemComponent.cs`
- Create: `src/Items/WorldItemReservationComponent.cs`
- Create: `src/Items/TileEntityBindingComponent.cs`
- Create: `src/Items/ShopInventoryComponent.cs`
- Create: `src/Items/ShopOffer.cs`

Implement only the approved fields, constructors, and derived properties. Clone incoming mutable
collections and expose them through read-only interfaces.

### Task 3: Add economy state components

**Files:**
- Create: `src/Items/Commerce/CurrencyBalanceComponent.cs`
- Create: `src/Items/Commerce/CommerceLedgerComponent.cs`
- Create: `src/Items/Commerce/CommerceLedgerEntry.cs`
- Create: `src/Items/Commerce/CommerceTransactionKind.cs`
- Create: `src/Items/Commerce/CommerceTransactionState.cs`

Keep balance and ledger state separate. The component layer does not calculate a price or alter a
balance.

### Task 4: Add loot source state components

**Files:**
- Create: `src/Items/Loot/LootSourceComponent.cs`
- Create: `src/Items/Loot/LootAttributionComponent.cs`
- Create: `src/Items/Loot/LootSourceKind.cs`

Store only source selection, single-resolution state, and attribution snapshots. Do not persist a
random-roll result.

### Task 5: Compile the affected project serially

**Files:**
- Verify: `src/Items/Terraria.Items.csproj`

Before building, inspect live `dotnet.exe` and `csc.exe` owners. Invoke the repository wrapper in
the active PowerShell session with `-DotnetArguments` so MSBuild arguments are forwarded:

```powershell
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build', '.\src\Items\Terraria.Items.csproj', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false')
```

Confirm exit code, warning/error counts, and the expected artifact under
`Build/bin/Terraria.Items/`.

No test files or test commands are included: the user explicitly excluded testing for this
component-declaration task.
