# World Item Pickup Authority Slice

`DomeSimulation.CommitWorldItemPickups` now resolves the player handle and requires its
`PlayerLifecycleComponent.IsActive` flag before running inventory transfer. An inactive player can
submit a syntactically valid pickup command, but it produces no inventory mutation, no world-item
deactivation and no pickup event. This card covers only active-player authority; range, delay,
capacity, stacking and persistence remain separate contracts.

The focused Items verifier kills a player, submits a pickup command, and proves the item remains
active and no pickup event is published.
