# World Item Pickup Range Integrity Slice

`WorldItemPickupSystem.TryPickup` now rejects non-finite or negative pickup ranges before distance
comparison. This prevents `NaN` from bypassing the range predicate and turning a distant world item
into an accepted pickup. The card covers only range input integrity; active-player authority,
distance semantics, pickup delay, inventory capacity and stacking remain separate contracts.
