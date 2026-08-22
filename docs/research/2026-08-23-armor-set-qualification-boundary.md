# ArmorSetBonuses 资格边界审计

本记录是 M-001 `Initialize_AlmostEverything` 的一个独立资格审计，不是将
`ArmorSetBonuses` 标记为已迁移，也不引入新的聚合 initializer。

## Source oracle

- `D:\TRbackup\Version4物理删除了某些文件\Terraria.DataStructures\ArmorSetBonuses.cs`
  - SHA-256: `E5B960C22E731357D79A444E6438B53055344B394C6C140F4796C8D4136E8DF3`
  - `All`/`SetsContaining`: lines 516-518
  - `Initialize`: lines 520-615
  - `BuildLookup`: lines 616-643
  - `GetCompleteSet`: lines 644-665
  - `Add` overloads: lines 674-687
- `D:\TRbackup\Version4物理删除了某些文件\Terraria.DataStructures\ArmorSetBonus.cs`
  - SHA-256: `03FFA6BCB76A32F58B2D765A88AA8746DC5E29FFC5C9EBCED847FFEC8E13A52F`
  - `QueryContext` reads the three equipped item types: lines 12-34
  - `Builder` expands set combinations and stores effect delegates: lines 48-126
  - `QueryCount` determines complete matches: lines 163-191

`Terraria.Main` calls `ArmorSetBonuses.Initialize()` and `BuildLookup()` at lines 3811-3812,
then `Player` calls `GetCompleteSet(...).Effect(this)` at line 9551. The latter means the
registration table is not an inert content catalog: it is on the authoritative player-effect
dispatch path.

## Qualification result

| Predicate | Result | Evidence |
|---|---|---|
| bounded registration calls | yes | `Initialize` contains many `Create(...).Set(...).Add()` and `Add(...)` calls |
| fixed identity-only definition | no | each entry stores an `ArmorSetEffect` delegate and localized description |
| unique Simulation owner | no | effects mutate player combat, movement, mana, buffs, flags, achievements and lighting |
| deterministic matching contract | partial | `QueryContext`/`QueryCount` are deterministic, but duplicate/order semantics come from `All` and lookup overwrite behavior |
| persistence contract | no | equipped item state exists, but set-effect state and transient counters are not modeled as a separate snapshot contract |
| replay contract | no | effect callbacks include mutable player state and achievement/presentation side effects |
| protocol/client boundary | no | localized descriptions, lighting and achievement callbacks cross presentation/client concerns |

## Decision

`ArmorSetBonuses.Initialize/BuildLookup` remains `unknown/deferred` for M-001. No
`ArmorSetDefinitionRegistry`, generic `Initialize_AlmostEverything`, or effect queue is added.
The existing item/equipment work may continue to own raw equipped item identity and ordinary
equipment stat projection, but it must not claim armor-set parity.

## Required prerequisites for a future child

1. A typed armor-set matching definition with explicit duplicate and precedence semantics.
2. A replayable player capability/effect command model that separates authoritative stats from
   achievements, lighting, localized text and other presentation effects.
3. Snapshot/persistence fields for any stateful effect (for example cooldowns or set flags).
4. A source-backed verifier covering complete, incomplete, conflicting and repeated equipment
   transitions, plus a two-session protocol projection where armor-set state is visible.

Until these prerequisites exist, extracting only the item triples would create a false sense of
coverage and would lose the source-owned effect dispatch contract.
