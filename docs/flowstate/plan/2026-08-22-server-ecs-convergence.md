# Server ECS Convergence Remaining Work

**Strategy:** `graph`
**Iteration:** Server ECS convergence remaining work
**Scope source:** `docs/plans/2026-08-22-server-ecs-convergence-remaining-work.md`
**Status:** in_progress

## Objective

Drive the remaining server-authoritative ECS convergence work without deleting the legacy
WorldGen or source oracle before source-backed parity and deletion gates are true. Every
accepted capability must have an owner, mutation path, persistence/protocol consequence,
and clean-process evidence.

## Phases and dependencies

1. **P1: Contract and evidence baseline** — refresh current gate metadata and freeze the
   source/runtime inventories. This is the prerequisite for all semantic claims.
2. **P2: WorldGen contract and differential** — recover supported-profile source predicates,
   runtime state, random checkpoints and stage fingerprints. Depends on P1.
3. **P3: Physical deletion ledger** — audit every ServerRelevant row against the complete oracle;
   promote only source-backed replacements or client-only classifications. Depends on P1 and
   the WorldGen findings from P2.
4. **P4: TrainingDummy authority boundary** — prove an inbound placement contract or record a
   source-backed deferred boundary. Depends on P1; P2 is informative where tile identity is used.
5. **P5: Final acceptance review** — run fresh clean-process verifiers, builds, differential and
   deletion gates, then publish one final conclusion only if every completion condition is true.
   Depends on P2, P3 and P4.

## Non-negotiable boundaries

- `canRemoveLegacyWorldGen` remains false until tile, extended state, metadata, command sequence
  and random checkpoints all match the complete oracle.
- `ServerRelevant` rows remain `deferred` unless their full replacement chain is evidenced.
- A server-to-client packet is not evidence of a client-to-server mutation contract.
- Focused verifier counts and weighted evidence scores never substitute for semantic parity.

## Current state

P1 has a current full regression summary and Version4 ledger, but historical/stale references
remain visible. P2 is blocked by the complete differential mismatch; P3 has 44 deferred
ServerRelevant rows; P4 is partial; P5 is not entered.
