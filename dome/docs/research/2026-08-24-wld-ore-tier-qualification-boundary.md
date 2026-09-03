# WLD Ore-Tier Qualification Boundary

This card records why the saved ore-tier group remains blocked. It does not add a fabricated
default or infer a tier into the existing `WorldMetadata` identity owner.

## Source Facts

The Version4 source reads the direct group only for v216+:

- `Terraria.IO.WorldFile.LoadHeader`, `WorldFile.cs:2184-2187` reads saved Copper, Iron,
  Silver and Gold tile IDs.
- For versions before v216, the source explicitly sets all four values to `-1`:
  `WorldFile.cs:2188-2192`.
- After load, `CheckSavedOreTiers()` (`WorldFile.cs:811-869`) checks whether all four values
  are present. If any value is missing, it counts paired tile types in the loaded world and
  selects the dominant vanilla/alternate tile ID for each tier.
- The source writes the four values at `WorldFile.cs:1426-1429`.

## Current Disposition

`blocked`, not `accepted` or `candidate`:

- `WorldMetadata` is a direct world identity owner, while ore tiers are world-generation
  compatibility facts that can be reconstructed from tiles in some historical layouts.
- The current immutable world model has no typed `SavedOreTiers` owner and no source-backed
  post-import tile-count repair phase.
- A v1-v215 import cannot distinguish “source omitted the values” from a real saved tier without
  executing the source repair predicate.
- Returning `0`, vanilla IDs, or seed-derived IDs would fabricate state and could change tile
  generation, mining rules, or downstream content selection.

The parser still consumes the direct v216+ group and preserves offsets. It intentionally does not
project the values into compatibility or Simulation state. A future card must first define a
named immutable ore-tier owner, exact version predicates, tile-count repair inputs, persistence
versioning, and any protocol projection before importing either direct or repaired values.

## Accepted Evidence Boundary

- Header version matrix passes v1-v319 layout and offset preservation.
- Existing parser fixtures prove later fields remain aligned after the ore group.
- No runtime ore-tier state or default was added by this card.

This is a qualification/blocked card, not a claim of WLD ore parity.
