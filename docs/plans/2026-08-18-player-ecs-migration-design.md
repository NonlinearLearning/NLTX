# Player ECS Migration Design

## Decision

Migrate the legacy `Terraria.Player` behavior through server-authoritative,
capability-oriented vertical slices. The legacy file at
`D:\TRbackup\Version4物理删除了某些文件\Terraria\Player.cs` is a read-only
behavior baseline and evidence source; it is not copied into `src/` and is not
made a runtime dependency of the new simulation.

The first delivery covers the authoritative player core: identity and
lifecycle, input, movement and collision, health and mana, damage/death/
respawn, inventory/equipment projections, player commands, deterministic
snapshots, and V1.4.5 protocol projections. Client-only presentation and
specialized Terraria mechanics remain explicitly staged follow-up work.

## Context and Evidence

The legacy `Player.cs` is approximately 634 KB and contains roughly 448 method
or member declarations. Its responsibilities cross simulation, persistence,
networking, world interaction, and client presentation. Representative groups
include:

| Legacy responsibility | Examples in `Player.cs` | Target ownership |
| --- | --- | --- |
| Identity and persistence | profile, UUID, loadout, save/load helpers | `Player` persistence records and server import/restore |
| Input and control | `Update`, `UpdateControlHolds`, `HorizontalMovement`, `JumpMovement` | `Player` input/control systems |
| Motion and collision | gravity, tile collision, slope, rope, liquid, wings | `Movement` and `Physics` systems |
| Vitals and combat | `Hurt`, `KillMe`, `HealEffect`, `ManaEffect`, immunity | `Combat` components/systems and player lifecycle |
| Inventory and item use | `GetItem`, `ItemSpace`, `ConsumeItem`, `ItemCheck`, mana payment | `Inventory` components/systems and commands |
| Equipment and buffs | `UpdateEquips`, armor sets, `AddBuff`, `UpdateBuffs` | `Inventory` and `StatusEffects` domains |
| World interaction | tile reach, chests, doors, signs, pickup, teleport | interaction commands and world systems |
| Replication | player slot, vitals, controls, equipment and bootstrap state | protocol/server projection layer |
| Presentation | drawing, dust, audio, camera, visual accessories | client adapter; excluded from simulation core |

The current NLTX baseline already contains `PlayerSnapshot`,
`PlayerStateSnapshot`, `PlayerInputComponent`, `PlayerControlStateComponent`,
`PlayerLifecycleComponent`, player control/input/gravity systems, persistent
player records, and player authority verification. The design extends these
boundaries instead of introducing a second player runtime.

## Architecture

The runtime model is:

```text
Player entity
  + PlayerTagComponent
  + PlayerIdentityComponent
  + PlayerLifecycleComponent
  + PlayerInputComponent
  + PlayerControlStateComponent
  + TransformComponent
  + VelocityComponent
  + ColliderComponent
  + PhysicsStateComponent
  + HealthComponent
  + ManaComponent
  + InventoryComponent
  + EquipmentLoadoutComponent
  + BuffCollectionComponent
```

Systems read components and immutable world/simulation snapshots, then emit
commands or events. Commands are applied at a deterministic commit boundary.
Protocol code reads explicit player projections and never writes ECS state
directly. Persistence records are account-level state and are projected into a
runtime entity when a session creates or restores a player.

The simulation must not reference legacy `Main`, `Player`, `NPC`,
`MessageBuffer`, `NetMessage`, XNA types, UI types, audio types, or client
rendering services. Those concerns remain in server, protocol, compatibility,
or client assemblies.

## Component Boundaries

### Player domain

- `PlayerTagComponent`: zero-sized entity classification marker.
- `PlayerIdentityComponent`: stable `PlayerHandle`, assigned network slot, and
  canonical account UUID. It contains identity only, not mutable vitals.
- `PlayerLifecycleComponent`: active state, respawn countdown, and spawn point.
- `PlayerInputComponent`: one-tick intent (`MoveLeft`, `MoveRight`, `Jump`,
  `Fire`, `UseItem`). It is replaced at input ingress and never persisted.
- `PlayerControlStateComponent`: cooldowns, held-control state, and control
  locks that affect rule evaluation.
- `PlayerInteractionComponent`: validated interaction target and current
  interaction mode. It is created only when interaction work is introduced.
- `PlayerMountStateComponent`: mount identity and movement mode. It is a later
  slice and must not be added to the first core solely as a placeholder.

### Shared capability domains

- `TransformComponent` and `VelocityComponent`: spatial state.
- `ColliderComponent` and `PhysicsStateComponent`: collision shape and contact
  state.
- `HealthComponent`: current and maximum health for any damageable entity.
- `ManaComponent`: current/max mana, regeneration delay, and bounded regen
  accumulator.
- `DefenseComponent` and `ImmunityComponent`: damage calculation inputs and
  post-hit protection windows.
- `BuffCollectionComponent`: bounded runtime status effects; persistent buff
  records remain in `PlayerPersistentState` until imported.

### Inventory and equipment domains

- `InventoryComponent`: supported runtime slots used by item systems.
- `SelectedItemComponent`: selected runtime slot and selection revision.
- `EquipmentLoadoutComponent`: equipped item references and loadout index.
- `ItemUseStateComponent`: use animation/cooldown/channel state. It is not a
  general-purpose player state bag.

The existing `PlayerPersistentState` remains a persistence/import DTO. It is
not folded into one large ECS component and it retains unsupported protocol
equipment rather than truncating it to the current runtime inventory range.

## System and Command Boundaries

The first stable tick order is:

1. `PlayerInputApplySystem` clears and applies the authoritative input batch.
2. `PlayerControlSystem` converts input into movement intent, facing, jump,
   and typed action requests.
3. `PlayerGravitySystem` and movement systems update velocity.
4. `TileCollisionSystem` and `GroundCollisionSystem` resolve contacts.
5. `PlayerVitalRegenSystem` applies bounded life/mana regeneration.
6. `DamageResolutionSystem` applies queued damage commands.
7. `PlayerDeathSystem` emits death facts and starts respawn state.
8. `PlayerRespawnSystem` consumes `RespawnPlayerCommand` at the configured
   boundary.
9. `BuffDurationSystem` and `BuffEffectSystem` update status effects.
10. `PlayerItemUseSystem`, inventory transfer, and interaction systems consume
    validated action commands.
11. Replication projections read the post-commit snapshot.

Commands represent intent, including `DamagePlayerCommand`,
`RespawnPlayerCommand`, `UseItemCommand`, `PickupWorldItemCommand`, and future
typed interaction commands. Events represent committed facts, such as
`PlayerDamagedEvent`, `PlayerDiedEvent`, `PlayerRespawnedEvent`, and
`PlayerItemUsedEvent`. A command must not be used as a durable event, and a
protocol packet must not bypass the command boundary.

## Data Flow and Trust Boundaries

```text
client packet
  -> protocol parser
  -> session ownership/shape validation
  -> typed server command
  -> simulation tick
  -> ECS systems
  -> deterministic command commit
  -> immutable PlayerSnapshot / PlayerStateSnapshot
  -> protocol projection
  -> client packet
```

Connection bootstrap is special: the server accepts client bootstrap state only
for a previously unknown canonical UUID, then stores the imported record. For
an existing UUID, client bootstrap values are parsed for conformance but the
server record wins. Active-session packets cannot mutate persistent equipment,
vitals, buffs, or loadouts through the bootstrap path.

## Non-Goals for the First Core

The first core does not implement client rendering, camera modifiers, dust,
audio, UI, map drawing, social cosmetics, golf, fishing visuals, or every
special movement mode. It also does not attempt to reproduce the entire legacy
method surface. Each excluded method must be assigned to a later domain or
recorded as intentionally client-only in the behavior map.

## Design Acceptance Criteria

- The simulation owns authoritative mutable player state.
- A player tick is deterministic for the same snapshot, input batch, and
  command order.
- Input, damage, death, respawn, inventory, and interaction changes cross typed
  command boundaries.
- Protocol projections can be tested without constructing a legacy `Player`.
- Persistent records preserve all imported protocol slots and unsupported item
  metadata, even when only a subset is executable at runtime.
- Every migrated legacy behavior has a status in the behavior map: migrated,
  delegated, intentionally client-only, or blocked with a named dependency.
- A phase can be disabled or rolled back without deleting the legacy behavior
  evidence or corrupting persisted player records.
