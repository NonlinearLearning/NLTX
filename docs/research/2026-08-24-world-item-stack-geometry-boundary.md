# World-item stack geometry boundary

## Owner

- `src/Terraria.Dome.Simulation/Items/Systems/WorldItemStackingSystem.cs`

## Accepted narrow predicate

The world-item merge owner rejects non-finite receiver/donor positions and non-finite maximum
distance before distance arithmetic or stack mutation. Finite compatible items retain the existing
replication-id ordering, section, delay, stack-limit and revision rules.

## Deferred source branches

The legacy 30-pixel Manhattan threshold, position/velocity interpolation, shimmer and passive
eligibility tables, owner cadence, network effects and full item lifecycle remain deferred.

## Verification

`Test/Terraria.Dome.Items.Verification` covers finite compatible merge and rejection of NaN item
position and NaN merge distance.
