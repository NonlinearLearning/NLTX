# NPC definition input boundary

`NpcDefinition` now rejects negative `LootTableId` values at construction. `0`
remains the explicit no-loot sentinel used by the immortal Training Dummy;
positive IDs continue to resolve through `NpcLootDefinitionRegistry`.
The constructor also enforces that `TownHome` uses `Town/Town` faction/category
and that `Segment` uses the `Segment` category. The built-in town fixture now
declares that authority explicitly.

This prevents an invalid negative table from entering the definition registry
and failing later during death/loot processing.

Evidence: `Build/diagnostics/npc-complete/task-1-definition/20260824-134500/`.
