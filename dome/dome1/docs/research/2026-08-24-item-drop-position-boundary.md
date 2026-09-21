# Item-drop position boundary

## Source and owner

- ECS owner: `src/Terraria.Dome.Simulation/Items/Systems/ItemDropRuleSystem.cs`
- downstream command owner: `WorldItemSpawnSystem`

The drop rule route creates authoritative world-item commands that are later committed by the
world-item spawn owner. A non-finite position cannot represent a valid legacy item/world position
and must be rejected before deterministic drop selection produces commands.

## Accepted narrow predicate

`ItemDropRuleSystem.Evaluate` now rejects NaN and infinite X/Y positions together with its existing
source identity, tick and spawn-source validation. Valid deterministic drops and weighted chains
remain unchanged.

## Deferred source branches

Complete legacy `Item.NewItem` source classification, item tables, shimmer/encumbrance, network
effects, random global ordering and client presentation remain deferred.

## Verification

`Test/Terraria.Dome.Items.Verification` covers deterministic repeated drops and non-finite position
rejection.
