# Legacy AiStyle 2 Design

## Goal

Migrate the legacy `aiStyle = 2` family as explicit authoritative behaviors, starting with the
source-backed generic `Projectile.type = 3` path.

## Source Boundary

`D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs` assigns type 3 to
`aiStyle = 2` at lines 578-586. Its generic AI branch increments `ai[0]`, applies no gravity
before tick 20, then applies `velocity.Y += 0.4f`, `velocity.X *= 0.97f`, and caps vertical
velocity at 32. Type-specific branches remain separate future behaviors.

## Architecture

- Add behavior ID 3 for this generic delayed-gravity state machine; `State.Primary` is the
  authoritative `ai[0]` timer and `State.Phase` remains the simulation tick.
- Add a typed replication projection for behavior ID 3 so V2/V3 NPC envelopes and player
  snapshots carry the timer without raw legacy `ai[]` access.
- Replace the current default type-3 fixture with the source-backed friendly definition.
  NPC hostile tests must supply an explicit hostile test definition rather than relying on a
  fake legacy type.
- Do not apply this behavior to type-specific legacy exceptions until their own source anchors,
  state fields, and fixtures exist.

## Acceptance

- Type 3 has the source dimensions, penetration, faction, and delayed-gravity behavior.
- Ticks 1-19 preserve velocity; tick 20 applies the source acceleration/drag; vertical speed
  cannot exceed 32.
- The timer survives authoritative player/NPC replication.
- Existing NPC hostile firing remains tested through an explicit fixture definition.
