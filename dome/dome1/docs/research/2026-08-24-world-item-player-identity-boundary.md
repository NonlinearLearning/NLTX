# World-item pickup player identity boundary

## Owner

- `src/Terraria.Dome.Simulation/Simulation/PlayerHandle.cs`
- `src/Terraria.Dome.Simulation/Items/Components/ItemWorldStateComponent.cs`
- `src/Terraria.Dome.Simulation/Items/Systems/WorldItemPickupSystem.cs`

## Accepted narrow predicate

World-item pickup authorization now rejects an invalid player handle before evaluating reservation
ownership. Valid unreserved players remain eligible, and reserved items remain restricted to the
matching valid player id. The handle value object exposes the shared positive-id contract.

## Deferred source branches

Full owner-selection cadence, inactive-owner reevaluation, shimmer, encumbrance, enemy pickup,
network reservation and persistence restart semantics remain deferred.

## Verification

`Test/Terraria.Dome.Items.Verification` covers reserved-owner acceptance, non-owner rejection,
invalid reservation rejection and invalid player-handle rejection.
