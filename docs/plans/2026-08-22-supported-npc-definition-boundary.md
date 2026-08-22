# M-001/M-019 supported NPC definition boundary

## Narrow proposition

The current supported NPC slice uses immutable `NpcDefinition` values and a typed
`NpcDefinitionRegistry`. Definition lookup, target selection, behavior state, death revision and
deterministic loot remain domain-owned and do not require legacy `Main.npc[]` or NPC subclasses.

## Source boundary

Version4 `Initialize_TileAndNPCData1/2` and NPC defaults cover the complete Terraria NPC table and
many event/boss/static flags. The current registry contains only supported fixture/chaser/town
definitions. Boss identity, event type mapping, AI families, spawn tables and full `SetDefaults`
parity remain deferred.

## Status

Accepted narrowly as a child of M-001 and M-019. It does not promote the full NPC lifecycle or the
mixed `Initialize_AlmostEverything` entry point.
