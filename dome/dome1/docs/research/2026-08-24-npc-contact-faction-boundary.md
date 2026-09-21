# NPC contact damage faction boundary

`NpcContactEffectSystem` now treats faction as authoritative input to contact
damage generation. Only `NpcFaction.Hostile` can emit a
`DamagePlayerCommand`; town and neutral NPCs are rejected before geometry is
converted into a combat command.

The Dome contact collection reads `NpcDefinitionComponent.Faction` from the
NPC entity. It no longer infers combat eligibility from `NpcBehaviorId` (for
example, `TownHome`), so behavior registration and combat allegiance remain
independent typed contracts.

The focused Combat verifier covers the existing hostile overlap path and direct
rejection of town and neutral candidates. This is a bounded contact-effect
slice; PvP policy, immunity ownership, and full collision family parity remain
in the shared Combat/Physics scope.
