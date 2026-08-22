# Main ECS migration M-003 item definition registration boundary

## Narrow proposition

The currently supported server item behaviors use immutable `ItemDefinition` values validated by
`ItemDefinitionRegistry` and `ItemDefinitionCompiler`. Registry construction rejects duplicate or
empty types, invalid stack limits, unknown dependent types, contradictory ammunition contracts and
invalid placement, recovery, equipment, combat and Extractinator metadata. Unknown item types remain
rejected by the existing lookup boundary.

## Source boundary

Version4 `Main.Initialize_Items` (`Main.cs:3915-3947`) instantiates and defaults every `ItemID` entry,
then populates staff/claw and other client-facing arrays. Those complete tables, visual arrays,
legacy `SetDefaults` coverage and UI/client metadata are not represented by the current server
Simulation registry and remain deferred.

## Owner and verification

Owner: `Terraria.Dome.Simulation.Items.ItemDefinitionRegistry` and
`Terraria.Dome.Simulation.Items.Definitions.ItemDefinitionCompiler`.

Focused verification: `Terraria.Dome.Items.Definitions.Verification` proves valid immutable records,
dependent references, rejection invariants, Version4 Extractinator mappings and deterministic output.

## Status

Accepted narrowly. Full `Initialize_Items` table coverage, staff/claw arrays, vanity/UI metadata and
all legacy item defaults remain open under the item-specific migration plan.
