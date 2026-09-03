# WorldGen Gems primary-loop boundary

The first Gems subloop iterates tile types `63..68`, each with its own
width-scaled count multiplier, retries a random stone candidate at most three
times, then schedules a TileRunner with strength `[2,6)` and steps `[3,7)`.

`LegacyGemsPass` owns that scheduler and retains the Skyblock no-op. Its primary
requests use the same random stream for TileRunner traversal and are committed
through typed tile commands. `LegacyGemsSandShiftPass` now models the later
source-order loop: both directional scans, the explicit conversion sand registry,
Underground Desert exclusion, the ten-tile `InWorld` inset, terminal downward
placement, and source deactivation with projected command state. The pass is
caller-owned and is not integrated into `WorldGenerationPipeline` because the
current request contract does not provide authoritative Underground Desert bounds.

The focused scratch boundary verifies primary traversal, both sand-shift
directions, terminal placement, and Desert exclusion. Aggregate ordering, WLD
parity, complete traversal/RNG parity, pipeline integration, and legacy deletion
remain deferred.
