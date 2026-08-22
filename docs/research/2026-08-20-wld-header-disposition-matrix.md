# WLD Header Disposition Matrix

This matrix records the current disposition of every Header value consumed by the two WLD
header readers. Version ranges are inclusive. `accepted` means the value has one authoritative
immutable owner; `candidate` means a direct destination exists but the runtime contract still
needs a separate card; `blocked` means source semantics or prerequisites are missing;
`client-only` means it must not enter the server Simulation.

| Wire group / source read | Version range | Destination | Disposition | Evidence or reason |
|---|---:|---|---|---|
| name, worldId, bounds, width, height | v1+ | `WorldMetadata` | accepted | Direct identity and geometry owner |
| game mode integer / legacy expert boolean | v112+ / v208 | `WorldRuleState.GameMode` | accepted | Version parser and projection tests |
| UUID | v181+ | `WorldMetadata.UniqueId` | accepted | Direct immutable world identity; projected to V1456 WorldData and v30 optional persistence tail. Map filename/client storage remain deferred. Evidence: `docs/research/2026-08-24-wld-uuid-identity-boundary.md` |
| world generator version | v179+ | `WorldMetadata.WorldGeneratorVersion` | accepted | Nullable direct identity metadata; v29 persistence tail field; missing pre-v179 values remain unknown |
| textual seed input | v179+ | `WorldMetadata.SeedText` | accepted | Raw text is preserved through compatibility and v31 persistence; secret-seed parsing/repair and protocol exposure remain deferred. Evidence: `docs/research/2026-08-24-wld-seed-text-qualification-boundary.md` |
| world surface / rock layer | v1+ | `WorldMetadata` | accepted | Direct world-generation metadata |
| time double / day-time boolean | v1+ | `WorldClock.TimeOfDay` / `WorldClockSnapshot` | accepted | Versioned fractional clock preserves the exact Double; V1456 integral quantization is projection-only. Evidence: `docs/plans/2026-08-22-wld-clock-fraction-boundary.md` |
| moon phase | v1+ | `WorldClockSnapshot.MoonPhase` | accepted | Range validated 0..7 |
| blood moon / eclipse | v1+, v70+ | `WorldProgressionState` | accepted | Explicit progression flags |
| boss and hard-mode booleans | versioned | `WorldProgressionState` | accepted | Direct named flags and version tests |
| goblin / frost / pirate / Martian flags | v29+ / v37+ / v56+ / v131+ | `WorldProgressionState` | accepted | Indexed source groups are documented and tested |
| meteor pending flag | v1+ | `WorldProgressionState.IsMeteorScheduled` | accepted | Direct saved schedule only |
| invasion size/type/X | v1+ | `WorldProgressionState` | accepted | Direct facts; runtime lifecycle remains separate |
| rain activity / duration / maximum strength | v53+ | `WorldRuleState` raw fields | accepted | Direct triple maps losslessly to independent activity, duration and maximum-strength facts; the secret-seed repair branch remains separately blocked. Evidence: `docs/plans/2026-08-22-wld-rain-runtime-boundary.md` |
| ore tiers | v216+ direct, pre-v216 source repair | none | blocked | `WorldFile.CheckSavedOreTiers` requires a typed saved-tier owner plus tile-count repair phase; parser keeps alignment only. Evidence: `docs/research/2026-08-24-wld-ore-tier-qualification-boundary.md` |
| background ids, cloud state/count | versioned | none | client-only | Presentation/random weather state |
| wind target | v62+ | `WorldRuleState.WindSpeedTarget` | accepted | Direct modern restoration; old random branch deferred |
| secret-seed repair membership | source-dependent | none | blocked | `FixEndlessRainWorlds` depends on seed-text parsing and the source secret-seed registry; compatibility snapshot has neither. Evidence: `docs/research/2026-08-24-secret-seed-rain-repair-boundary.md` |
| later banners, parties, spawn points and legacy counters | v95+ | none or client systems | no-owner | No unique server owner; keep parser alignment only |

## Fixture Assertions

The parser verifier supplies source-position fixtures for the direct candidates and version
boundaries:

- `VerifyWorldRainVersionBoundary` asserts the v53+ rain triple and the v87 legacy boundary.
- `VerifyWorldWindVersionBoundary` asserts the v62 wind boundary.
- `VerifyPointerWorldProgressionFacts`, `VerifyWorldMeteorSchedule` and the event-flag tests
  assert the indexed progression fields.
- `VerifyLegacyVersionMatrix` and `VerifyPointerVersionMatrix` assert every supported layout
  dispatch and preserve reader offsets through the remaining Header groups.

This is an accounting artifact, not a claim that blocked or no-owner fields are recovered.
