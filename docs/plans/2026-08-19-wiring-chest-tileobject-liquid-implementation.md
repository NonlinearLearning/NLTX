# Wiring, Chest, and TileObject Liquid Implementation Plan

## 1. Wiring Light Definitions

1. Add a definition type and source-derived registry for the approved light families.
2. Add a frame-command builder that derives the top-left tile and emits deterministic
   footprint frame changes.
3. Keep the existing single-tile lamp behavior compatible while routing definition-backed
   lamps through frame commands.
4. Add focused RED cases for each footprint shape, forced state, toggle state, and an
   invalid definition/hit rejection.
5. Implement the smallest behavior that makes the cases pass, then run the wiring and
   loopback verifiers.

## 2. Chest Locks and Keys

1. Locate the authoritative inventory consumption operation and keep key lookup there.
2. Add a lock-definition value type plus a constrained world-chest policy.
3. Extend the open command/system with the inventory dependency so validation precedes all
   mutations.
4. Persist and snapshot `IsLocked` and restore it before allowing opens.
5. Add RED cases for no key, wrong key, correct consumed key, non-consuming key, occupied
   chest, range failure, and restart persistence.
6. Implement and run Chest, persistence, and loopback verifiers.

## 3. TileObject Liquid Rules

1. Add a rule value type containing liquid kind, frame/style predicate, footprint, and
   frame-period origin calculation.
2. Add a registry whose explicit rule takes precedence over the global tile definition.
3. Route Liquid propagation contact evaluation through the resolver and retain deferred
   `TileChangeCommand(Kill, PreserveLiquid=true)` output.
4. Add RED cases for global fallback, object override, unsupported object style rejection,
   2x2 footprint ordering, and liquid preservation at commit.
5. Implement and run Liquid focused, world mechanics, and loopback verifiers.

## Final Evidence

Run the simulation build from repository root with `-p:UseSharedCompilation=false`, the
focused family verifiers, applicable persistence and loopback verifiers, and `git diff
--check`. Record commands, exit codes, and warnings in a fresh diagnostics artifact.
