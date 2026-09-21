# ContentSamples item repair 资格边界审计

## Source oracle

- Path: `D:\TRbackup\Version4物理删除了某些文件\Terraria.ID\ContentSamples.cs`
- SHA-256: `2E2598D787248C6BA759EFB58804859B8EE3E8B9C942CED65A5E8BA5AAF8FFB7`
- `FixItemsAfterRecipesAreAdded`: lines 937-946
- Source behavior: enumerate every `ItemsByType` entry and call
  `Item.Refresh(onlyIfVariantChanged: false)` after recipes are added.
- Initializer call: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs:3823`

## Qualification result

| Predicate | Result | Evidence |
|---|---|---|
| bounded method | yes | one deterministic full-catalog loop |
| identity-only definition | no | invokes mutable `Item.Refresh` on every item after recipe setup |
| unique existing owner | no | current item authority covers selected definitions/use/equipment, not complete recipe/variant refresh |
| source-complete input | no | requires full `ItemsByType`, recipe ordering and variant mutation semantics |
| persistence/replay contract | no | refresh timing and resulting item state are not represented as a versioned command or snapshot event |
| client/content boundary | partial | `ContentSamples` also owns creative/bestiary sorting, while this method mutates shared item runtime samples |

## Decision

`ContentSamples.FixItemsAfterRecipesAreAdded` remains `unknown/deferred`. Do not add a generic
post-recipe repair pass to Simulation and do not classify the existing item verifier as proof of
full catalog repair. A future card must first define recipe completion ordering, item variant
ownership, refresh idempotence and persistence/replay semantics.

## Prerequisites

1. Complete supported `ItemsByType`/recipe source inventory and deterministic setup ordering.
2. Typed variant-refresh command or immutable catalog rebuild boundary.
3. Tests for unchanged, changed and repeated refresh calls, including old snapshot recovery.
4. Explicit separation of server item runtime state from client creative/bestiary catalogs.
