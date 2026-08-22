# Item Extractinator ECS Design

## Status

Approved on 2026-08-19. This is an exact Version4 rule-distribution migration into the
server-authoritative Item ECS domain. It does not claim byte-for-byte compatibility with the legacy
global `Main.rand` stream.

## Goal

Migrate the authoritative Extractinator behavior represented by Version4 item definitions, player
use, wiring/chest activation, `ItemID.Sets.ExtractinatorMode`,
`Terraria.GameContent.ExtractinatorHelper`, and `ItemTrader.ChlorophyteExtractinator` into
`Terraria.Dome.Simulation` without importing legacy `Item`, `Player`, `Chest`, `Main`, or UI types.

## Source Baseline

The complete implementation oracle is the read-only source tree
`D:\TRbackup\无任何删减通过编译`:

- `Terraria.GameContent/ExtractinatorHelper.cs`: output branches, odds, stack increments, and ore
  pools.
- `Terraria.GameContent/ItemTrader.cs`: Chlorophyte Extractinator deterministic trades.
- `Terraria/Player.cs:42116`: direct player interaction order and target validation.
- `Terraria/Wiring.cs:2607`: wiring frame normalization, 60-tick cooldown, chest lookup and reverse
  slot scan.
- `Terraria.ID/ItemID.cs:1104`: the complete `ExtractinatorMode` input mapping.

The retained Version4 and Version3 source trees are incomplete for this behavior: their
`Wiring.Extractinator` body is a stub. They remain useful Item migration baselines but not a full
rule oracle.

## Scope

### Direct Player Use

The client submits only a player, source inventory slot, target cell and sequence. Simulation
validates the player entity, selected inventory slot, interaction range, active target tile, and
the target type. Only tile `219` (Extractinator) and tile `642` (Chlorophyte Extractinator) are
valid.

For a tile `642`, the immutable Chlorophyte trader is evaluated first. A matching trade consumes
the original source quantity and creates its fixed output. If no trade matches, the complete
`ExtractinatorMode` output algorithm is evaluated. A nonnegative mode produces exactly one output
attempt and consumes one input unit only when an output exists.

### Wiring and Chest Use

Wiring enters a separate command path. It normalizes a multi-tile Extractinator coordinate from
frame offsets, limits the normalized target to one activation per 60 ticks, locates the first
unlocked chest in the Version4 search rectangle, then scans the chest slots from highest to lowest.
It processes only the first source item that yields an output, consumes one source unit and creates
one world item at the normalized Extractinator origin.

This path belongs jointly to the wiring and chest owners. It must not be modeled as a player
`UseItemCommand`, and it must not require an online player or client packet.

## Domain Model

`ItemExtractinatorDefinition` is immutable per input item and contains its Version4 extraction
mode. The `ItemDefinitionRegistry` validates that the mode is `-1` or a supported mode `0` through
`6`.

`ExtractinatorRuleRegistry` is immutable and contains:

- the complete Version4 item-type to mode mapping;
- Chlorophyte reciprocal, cyclic and many-to-one trade rules;
- named result pool constants.

`UseExtractinatorCommand` contains `PlayerHandle`, source slot, target X/Y and a sequence.
`TriggerExtractinatorCommand` contains a target X/Y and a sequence. Committed success is represented
by `ExtractinatorResultEvent`, which includes source kind, input, output, target and sequence.

`ExtractinatorSystem` has no ECS or protocol dependency. It validates an immutable input definition,
selects a deterministic result, and returns a named result object. `DomeSimulation` owns all entity,
inventory, chest, world-item, range, cooldown and event mutations.

## Randomness

The system ports every Version4 branch, denominator, nested quantity increment, item identifier and
ore pool. It replaces `Main.rand` with a private deterministic random stream seeded from world seed,
tick, command sequence, input type, target coordinates and a source discriminator. Each source call
uses the next value in that stream, preserving the legacy branch order and distribution.

This produces replayable server results and stable multi-node behavior. It intentionally does not
attempt to reproduce the global legacy `Main.rand` call order, because unrelated legacy simulation
calls are not part of the Dome authority model.

## Transaction Rules

1. Determine the output before modifying inventory or chest state.
2. Verify that the output item definition exists and its stack quantity is valid.
3. Consume input once only after all acceptance conditions have passed.
4. Player output first transfers through the authoritative inventory transfer rules. Any remainder
   becomes an authoritative world item at the target position.
5. Wiring output always becomes an authoritative world item at the normalized target position.
6. A rejection changes no inventory/chest slot, revision, cooldown or committed-event buffer.
7. The same target accepts at most one successful Extractinator command per tick, ordered by sequence.

## Persistence and Projection

No legacy random state is serialized. The input tuple creates the deterministic random stream, so
replay requires only ordinary world seed, tick and command state. Existing inventory, chest and world
item persistence and snapshot/replication paths carry committed state. The transient event buffer and
per-tick winner set are not persisted.

## Exclusions

This slice excludes client sound, mouse position, UI feedback, legacy `Main` arrays, client-side
`ItemTime` animation timing, and arbitrary `ItemTrader` blocks outside tile `642`. It also excludes
other Deferred Item domains such as sentries, DD2, minecart tracks and shops.

The Item member `MakeUsableWithChlorophyteExtractinator` moves from Deferred to a Definition/System
owner after the player direct-use path is verified. The Wiring/chest invocation is tracked by its
existing Wiring/Chest ownership and does not turn all wiring behavior into an Item responsibility.
