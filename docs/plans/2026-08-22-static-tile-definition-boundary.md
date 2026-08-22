# M-001 static Tile definition boundary

## Narrow proposition

`TileDefinitionRegistry.CreateVersion4Base()` provides an immutable source-derived registry covering
all 753 Version4 Tile IDs. The current supported liquid and collision consumers read this registry
for solid/platform/no-attach and water/lava destruction facts, with unknown definitions failing
closed at the consuming boundary.

## Source boundary

This card covers only the static Tile-definition subset of `Initialize_TileAndNPCData1/2`. It does
not claim migration of the legacy mutable `Main.tileSolid`, `tileSolidTop`, `tileBlockLight`,
framing tables, animation tables, multi-tile data or all NPC static tables.

## Status

Accepted narrowly as a child of M-001. The mixed `Initialize_AlmostEverything` entry point remains
planned until its remaining domain families have independent owners.
