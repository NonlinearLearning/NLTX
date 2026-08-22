# Initializer Responsibility-Family Inventory

Source oracle: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs:3732-3859`.
The existing source-order matrix in `docs/research/2026-08-22-initialize-almost-everything-owner-matrix.md`
was re-audited against the current Simulation owners.

## Result

No new independent initializer responsibility family is proven by the current source and runtime
contracts. The unresolved calls are either client/content-only, already covered by a narrow child
owner, or lack a complete server predicate and restart/observable contract. In particular:

- `LeashedEntity.Registry.RegisterAll` lacks a recovered server lifecycle owner.
- `NPCInteractions.Initialize` lacks a source-backed supported interaction family.
- `FishDropRuleList` population lacks a complete server content owner.
- `PylonSystem`, shop setup, armor-set lookup, item repair, world-generation hooks and chat setup
  lack complete authority and persistence/projection contracts.

The source-order matrix remains the accounting authority. No aggregate
`Simulation.InitializeAlmostEverything` was added, and no unresolved row was promoted to `accepted`.

## Focused Predicate

An initializer family is eligible only when all of these are present:

1. a unique state owner in Simulation or an explicitly excluded client owner;
2. source-derived inputs and identity/range rules;
3. observable output or committed state;
4. restart/persistence or an explicit source-backed statement that it is process-local.

The current unresolved rows fail at least one of items 2-4. Treating a missing field as a default
would create an unsupported migration rather than close M-001.

## Boundary

This card is documentation/accounting evidence only. M-001 remains `deferred/planned` in the main
coverage matrix. The next executable candidates are the independently scoped M-007, M-008, M-009,
M-014 and M-024 contracts.
