# V1456 Compatibility Matrix

## Acceptance Boundary

This inventory is derived from the client-facing dispatcher in
`src/Terraria.Dome.Protocol.V1456/Dispatch/TerrariaPacketDispatcher.cs` and from the
frames emitted by `TerrariaPacketCodec`, `TerrariaV1456Compatibility`,
`TerrariaProtocolSessionHost`, and the server replication assemblers. `Handled` in
`TerrariaMessageCatalog` is not a grammar-completeness claim.

`Wire grammar` is one of `Unclassified`, `Partial`, or `Complete`. `Semantic support`
is one of `None`, `CompatibilityOnly`, or `Authoritative`. An `Unclassified` row is
deliberate compatibility debt: it may be framed, but it has not yet earned an original
source-derived grammar claim.

A normal frame that matches its V1456 grammar is never rejected solely because Dome lacks
the corresponding Simulation feature. The protocol rejects only an invalid frame-length
prefix, a truncated required or selected optional field, an impossible version-specific
layout, or an explicit server-authority validation failure with a controlled protocol
outcome. A rejected semantic action is parsed completely before its policy outcome is
chosen.

## Active-Path Inventory

`Trace evidence` is intentionally `Not captured` until a recorder artifact identifies a
real-client observation. `Dispatcher` means C2S acceptance; `Server emit` means a current
server path emits the frame.

| Id | Name | Direction | Original source | Fixed bytes | Conditional suffixes | Wire grammar | Semantic support | Field disposition | Raw-frame test | Trace evidence |
|---:|---|---|---|---:|---|---|---|---|---|---|
| 1 | Hello | C2S Dispatcher | `MessageBuffer.cs` case 1 | Variable .NET string | None | Complete | CompatibilityOnly | Bootstrap parser | `Terraria.Dome.Protocol.Compatibility.Verification` raw 15-byte frame | `v1456-compatibility-rerun-20260817-153000`, C2S sequence 3 |
| 3 | SetUserSlot | S2C Server emit | `NetMessage.cs` case 3 | 2 | None | Complete | CompatibilityOnly | Authoritative projection | `Terraria.Dome.Protocol.Compatibility.Verification` raw 5-byte frame | `v1456-compatibility-rerun-20260817-153000`, S2C sequence 4 |
| 4 | SyncPlayer | C2S Dispatcher / S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 4 | 36 + .NET string | None | Complete | CompatibilityOnly | Bootstrap parser | `Terraria.Dome.Protocol.Compatibility.Verification` raw 54-byte frame | `v1456-compatibility-rerun-20260817-153000`, C2S sequence 6 |
| 5 | SyncEquipment | C2S Dispatcher / S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 5 | 9 | None (`BitsByte` flags are fixed) | Complete | CompatibilityOnly | Bootstrap parser | `Terraria.Dome.Protocol.Compatibility.Verification` raw 12-byte frame | `v1456-compatibility-rerun-20260817-153000`, C2S sequence 12+ |
| 6 | RequestWorldData | C2S Dispatcher | `MessageBuffer.cs` case 6 | 0 | None | Complete | CompatibilityOnly | Bootstrap parser | Existing verifier | Not captured |
| 7 | WorldData | S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 7; `TreeTopsInfo.SyncSend`; `ExtraSpawnPointManager.Write` | `159 + world-name .NET string bytes + 4 * extra-spawn count` payload bytes | Always has a `byte` extra-spawn count followed by that many `Int16 X/Y` pairs; no flag-selected suffixes | Complete | CompatibilityOnly | `LegacyWorldDataContext` owns the complete source field order. Current Dome projection explicitly supplies zero/default values for unsupported world-state semantics while retaining all compatibility fields. | `Terraria.Dome.Protocol.Compatibility.Verification` source-shaped 172-byte all-fields frame | Not captured |
| 8 | SpawnTileData | C2S Dispatcher | `MessageBuffer.cs` case 8 | 9 | None | Complete | CompatibilityOnly | Request projection | Existing verifier | Not captured |
| 9 | StatusTextSize | S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 9 | 6 + `NetworkText` .NET string bytes (`Int32` count, mode, status flags) | Modes `Formattable` (1) and `LocalizationKey` (2) append a byte count and recursively serialized substitutions; `Literal` (0) has none | Complete | CompatibilityOnly | `LegacyNetworkText` recursively serializes every V1456 mode and source status-flags byte; current world stream intentionally projects literal text. | `Terraria.Dome.Protocol.Compatibility.Verification` source-shaped 29-byte literal and 51-byte nested frame | `v1456-compatibility-rerun-20260817-153000`, S2C sequence 366 |
| 10 | TileSection | S2C Server emit | `NetMessage.cs` case 10; `CompressTileBlock_Inner`; `DecompressTileBlock_Inner`; `TileEntity.Write` | Variable | Deflate body with tile RLE, then `Int16` chest/sign/entity counts | Complete | CompatibilityOnly | `LegacyTileSectionTile` expresses every source tile header/field and RLE branch. `LegacyTileEntity` variants cover all eight entity kinds emitted by the source TileSection scan, including item tuples and HatRack/DisplayDoll masks. Default `WorldGrid` remains an intentionally smaller Simulation projection. | `Terraria.Dome.Protocol.Compatibility.Verification` all-fields tile and entity raw frames; `Terraria.Dome.World.Protocol.Verification` sign-tail frame | Not captured |
| 12 | PlayerSpawn | C2S Dispatcher / S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 12 | 15 | None | Complete | Authoritative | Authority projection | Existing verifier | Not captured |
| 13 | PlayerControls | C2S Dispatcher / S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 13 | 14 | `flags2.bit2` velocity (8), `flags2.bit7` mount type (2), `flags3.bit6` return vectors (16), `flags4.bit5` camera target (8) | Complete | CompatibilityOnly | `PlayerControlIntent` authoritative; full legacy state session-local | `Terraria.Dome.Protocol.Compatibility.Verification` exit 0 | `v1456-compatibility-20260817-191100-quicklaunch`, C2S sequence 1402: 19-byte frame, 16-byte payload, `flags2=0x90`, mount type 3 |
| 14 | PlayerActive | S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 14 | 2 | None | Complete | Authoritative | Authority projection | `Terraria.Dome.Protocol.Compatibility.Verification` source-shaped 5-byte frame | Not captured |
| 16 | PlayerLifeMana | C2S Dispatcher / S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 16 | 5 | None | Complete | Authoritative | Bootstrap and authority projection | `Terraria.Dome.Protocol.Compatibility.Verification` raw 8-byte frame | `v1456-compatibility-rerun-20260817-153000`, C2S sequence 8 |
| 17 | TileManipulation | C2S Dispatcher / S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 17 | 8 | None | Complete | Authoritative | Authority projection | Existing verifier | Not captured |
| 18 | SetTime | S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 18 | 13 | None | Complete | CompatibilityOnly | Authoritative time with compatibility `sunModY` / `moonModY` zeroes | `Terraria.Dome.WorldRules.Verification` source-shaped 15-byte frame | Not captured |
| 19 | ToggleDoorState | C2S Dispatcher / S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 19 | 6 | `action` byte, `tileX` Int16, `tileY` Int16, `direction` byte | Complete | Authoritative | All actions 0-5 map to source-derived door, trapdoor, and tall-gate transitions. Simulation resolves the current object footprint; each accepted transition queues the original action, coordinates, and direction for PVS-limited observer replication, including multiple transitions in one server tick. | `Terraria.Dome.Protocol.Compatibility.Verification` raw actions 0-5; `Terraria.Dome.WorldMechanics.Verification`; `Terraria.Dome.WorldMechanics.Loopback.Verification` | Not captured |
| 20 | TileSquare | S2C Server emit | `NetMessage.cs` case 20 | 7-byte header + variable per-tile data | Every tile writes three flags bytes; colors, active type/frame, wall, and liquid/type follow their source predicates. No runs are used by message 20. | Complete | CompatibilityOnly | `LegacyTileSquareTile` represents every source-controlled conditional field. `WorldGrid` remains an explicit active/type/liquid-only Simulation projection. | `Terraria.Dome.Protocol.Compatibility.Verification` source-shaped 25-byte all-fields frame | Not captured |
| 21 | SyncItem | S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 21 | 24 | None | Complete | Authoritative | Authoritative item replication: `Int16` id, position/velocity vectors, stack, prefix, flags, and item type | `Terraria.Dome.Protocol.Compatibility.Verification` source-shaped 27-byte frame | Not captured |
| 23 | SyncNPC | S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 23 | 22 | Flag-selected AI0-3, player scaling, difficulty, compact life, and catchable release owner | Complete | Authoritative | `LegacyNpcWireState` projects all source-controlled fields; default authoritative snapshots use a conservative state, while compatibility inputs express every selected suffix. | `Terraria.Dome.Protocol.Compatibility.Verification` source-shaped 51-byte maximal frame | Not captured |
| 27 | SyncProjectile | C2S Dispatcher / S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 27 | 22 | `flags.bit0/1` AI0/AI1 `Single`; bit2 extended flags byte; bit3 banner `UInt16`; bit4 damage `Int16`; bit5 knockback `Single`; bit6 original damage `Int16`; bit7 UUID `Int16`; extended bit0 AI2 `Single` | Complete | Authoritative | All selected suffixes are consumed. C2S reported owner is socket-validated and the remaining client state is explicitly ignored; S2C replication is generated from authoritative projectile snapshots. | `Terraria.Dome.Protocol.Compatibility.Verification` source-shaped 50-byte all-suffix frame | Not captured |
| 29 | KillProjectile | C2S Dispatcher / S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 29 | 3 | None | Complete | Authoritative | `Int16` projectile identity and `byte` owner are parsed for C2S ownership validation and emitted from the authoritative projectile projection. | `Terraria.Dome.Protocol.Compatibility.Verification` source-shaped 6-byte frame | Not captured |
| 31 | RequestChestOpen | C2S Dispatcher | `MessageBuffer.cs` case 31 | 9 | None | Complete | Authoritative | Authority projection | Existing verifier | Not captured |
| 32 | SyncChestItem | S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 32 | 8 | None | Complete | Authoritative | Authority projection | `Terraria.Dome.Protocol.Compatibility.Verification` raw 11-byte frame | `v1456-compatibility-rerun-20260817-153000`, S2C sequence 369+ |
| 34 | SyncPlayerChest | C2S Dispatcher / S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 34 | 8 | None | Complete | Authoritative | Authority projection | Existing verifier | Not captured |
| 36 | SyncPlayerZone | C2S Dispatcher | `MessageBuffer.cs` case 36 | 7 | None | Complete | CompatibilityOnly | ExplicitlyIgnored after ownership validation | Existing verifier | Not captured |
| 40 | SyncTalkNpc | C2S Dispatcher | `MessageBuffer.cs` / `NetMessage.cs` case 40 | 3 | None | Complete | CompatibilityOnly | `byte` player slot is ownership-validated; `Int16` talk-NPC id is completely parsed then explicitly ignored because Dome has no NPC conversation authority. | `Terraria.Dome.Protocol.Compatibility.Verification` source-shaped 6-byte frame | Not captured |
| 42 | ItemRotationAndAnimation | C2S Dispatcher / S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 42 | 5 | None | Complete | CompatibilityOnly | Bootstrap parser | `Terraria.Dome.Protocol.Compatibility.Verification` raw 8-byte frame | `v1456-compatibility-rerun-20260817-153000`, C2S sequence 9 |
| 46 | OpenSignRequest | C2S Dispatcher | `MessageBuffer.cs` / `NetMessage.cs` case 46 | 4 | None | Complete | Authoritative | `Int16` tile X/Y request is parsed; the Simulation command resolves a PVS-visible sign and replies only to the requesting session with a source-shaped `OpenSignResponse`. | `Terraria.Dome.Protocol.Compatibility.Verification` source-shaped 7-byte frame; `Terraria.Dome.WorldObjects.Loopback.Verification` targeted response | Not captured |
| 47 | OpenSignResponse | C2S Dispatcher / S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 47; `Sign.maxSigns` | 8 + .NET string | None | Complete | Authoritative | `Int16` sign id/X/Y, string, player byte, and `BitsByte` are fully parsed. Simulation constrains generated IDs to the source V1456 range `0..31999`; C2S player byte is ignored in favor of socket identity; passive S2C PVS projections use player `255` and flag bit 0 to suppress UI opening. | `Terraria.Dome.Protocol.Compatibility.Verification` source-shaped 14-byte frame; `Terraria.Dome.WorldObjects.Verification` capacity boundary | Not captured |
| 49 | InitialSpawn | S2C Server emit | `NetMessage.cs` case 49 | 0 | None | Complete | CompatibilityOnly | Protocol completion marker | Existing verifier | Not captured |
| 50 | PlayerBuffs | C2S Dispatcher / S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 50 | 3 (`playerSlot` plus terminator) | Zero or more `UInt16` buff types before a final zero `UInt16` | Complete | CompatibilityOnly | Bootstrap parser | `Terraria.Dome.Protocol.Compatibility.Verification` raw 6-byte frame | `v1456-compatibility-rerun-20260817-153000`, C2S sequence 10 |
| 54 | NpcBuffs | S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 54 | 4 (`Int16` NPC id plus zero terminator) | Zero or more `UInt16` type / `UInt16` duration pairs before final zero `UInt16` | Complete | CompatibilityOnly | Default Simulation projection emits the empty terminator. `NpcBuffEntry` compatibility input emits every nonzero source type/duration pair without asserting buff Simulation authority. | `Terraria.Dome.Protocol.Compatibility.Verification` source-shaped 7-byte empty and 11-byte nonempty frames | `v1456-compatibility-rerun-20260817-153000`, S2C sequence 1326+ |
| 57 | WorldBiomeTypes | S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 57 | 3 | None | Complete | CompatibilityOnly | Compatibility projection | `Terraria.Dome.Protocol.Compatibility.Verification` raw 6-byte frame | `v1456-compatibility-rerun-20260817-153000`, S2C sequence 1353 |
| 60 | NpcHome | S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 60 | 7 | None; final `homeState` byte supports occupied (0), homeless (1), and town-NPC room assignment (2) | Complete | CompatibilityOnly | Default Simulation projection emits 0/1. The compatibility overload can emit every source `homeState`, including 2, without asserting town-room Simulation authority. | `Terraria.Dome.Protocol.Compatibility.Verification` source-shaped 10-byte state-0 and state-2 frames | `v1456-compatibility-rerun-20260817-153000`, S2C sequence 1359+ |
| 68 | PlayerUuid | C2S Dispatcher | `MessageBuffer.cs` / `NetMessage.cs` case 68 | Variable .NET string | None | Complete | CompatibilityOnly | Bootstrap parser | `Terraria.Dome.Protocol.Compatibility.Verification` raw 40-byte frame | `v1456-compatibility-rerun-20260817-153000`, C2S sequence 7 |
| 74 | AnglerQuest | S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 74 | 2 | None | Complete | CompatibilityOnly | Compatibility projection | `Terraria.Dome.Protocol.Compatibility.Verification` raw 5-byte frame | `v1456-compatibility-rerun-20260817-153000`, S2C sequence 1356 |
| 82 | NetModules | C2S Dispatcher / S2C Server emit | `MessageBuffer.cs` case 82 -> `NetManager.Read`; `NetworkInitializer.Load` registers IDs 0-14 | `UInt16` module id + module-specific payload | Source IDs 0-14 are Liquid, Text, Ping, Ambience, Bestiary, CreativePowers, CreativeUnlocks, TeleportPylon, Particles, CreativePermissions, Banners, CraftingRequests, TagEffect, LeashedEntity, and UnbreakableWallScan. | Complete | CompatibilityOnly | `NetModulePacket` consumes the complete frame boundary. Complete typed slices: ID 0 Liquid (`UInt16 count` + count x 6-byte update), ID 1 Text C2S (`ChatCommandId` .NET string + message .NET string) and S2C (`byte authorId` + recursive `NetworkText` mode/string/substitutions + RGB), ID 2 Ping (`Vector2`), ID 3 Ambience (`byte`, `Int32`, `byte`), ID 4 Bestiary (`byte subtype`, `Int16 npcNetId`, Kill-only 7-bit encoded `Int32 killCount`), ID 5 CreativePowers (registered source IDs 0-14: shared button no payload, shared toggle `bool`, shared slider `Single`, per-player toggle subtype 0 + 32-byte bitset or subtype 1 + `byte` + `bool`, and per-player slider `byte` + `Single`), ID 6 CreativeUnlocks (`byte reportedPlayerSlot`, `UInt16 itemId`, `UInt16 amount`), ID 7 TeleportPylon (`byte selector` 0-2, `Int16 x`, `Int16 y`, `byte type`), ID 8 Particles (`byte particleType`, two `Vector2`, `Int32`, `byte`), ID 9 CreativePermissions selector 0 (`byte 0`, `UInt16 powerId`, `byte level`; other selectors reject), ID 10 Banners (subtype 0 FullState `Int16 killCountLength` + `Int32[]` + `Int16 claimableCountLength` + `UInt16[]`; subtype 1 `Int16` + `Int32`; subtype 2/3 `Int16` + `UInt16`; subtype 4 `Int16` + `UInt16` + `bool`), ID 11 CraftingRequests C2S (`7-bit item count`, repeated `Int32 itemIdOrRecipeGroup` + `7-bit stack`, `7-bit chest count`, repeated `7-bit chest index`) and S2C (`bool approved`), ID 12 TagEffect (`byte owner`, subtype 0 + `Int16 effectType` + sparse `(byte npc,Int32 time)` entries terminated by `byte 200`, or subtype 1 + `Int16 effectType`, or subtypes 2-4 + `byte npc`), ID 13 LeashedEntity (subtype 0 + 7-bit slot; subtype 1/2 + 7-bit slot/type, Full-only `Int16 anchorX/Y`, then Kite or Critter state; Critter full types 7/11 append one type-specific byte), and ID 14 UnbreakableWallScan (`byte`, `bool`). ID 4 is dispatcher-validated but remains CompatibilityOnly because Bestiary state is not authoritative in Simulation. ID 5 / power 14 Journey spawn-rate remains owner/range-validated; all other complete ID 5 shapes are explicitly parsed then ignored because Dome has no matching Creative simulation authority. ID 10 is fully parsed then ignored because Dome has no Banner authority. ID 11 is fully parsed then ignored because Dome has no crafting authority. ID 12 is fully parsed then ignored because Dome has no tag-effect authority. ID 13 is fully parsed then ignored because Dome has no leashed-entity authority. Every registered V1456 module ID 0-14 now reaches its typed parser from an active session. All other module-specific payloads are explicitly ignored pending their grammar and Simulation authority. S2C join projections currently emit only IDs 0, 5, 9, and 10. | `Terraria.Dome.Protocol.Compatibility.Verification` raw opaque, Text C2S/S2C recursive modes plus malformed-tail/unknown-mode, Liquid, Ping, Ambience, Bestiary Kill/Sight/Chat plus malformed-tail/unknown-type, CreativePowers source IDs 0-14 plus unknown-ID/subtype/truncation/length boundaries, Banners default 293-entry FullState and subtypes 1-4 plus malformed type/truncation, CraftingRequests C2S 7-bit lists/S2C approval plus trailing/negative/truncated/invalid-encoding cases, TagEffect FullState sparse sentinel and subtypes 1-4 plus malformed sentinel/type cases, LeashedEntity Remove/Kite/registered Critter types 2-19 and full-only type 7/11 suffixes plus malformed type/truncation cases, CreativeUnlocks, TeleportPylon selector-2/unsupported-selector, Particles, CreativePermissions selector-0/unsupported-selector, and wall-scan frames; `Terraria.Dome.SessionReplication.Verification` exit 0 | Not captured |
| 101 | TowerShieldStrengths | S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 101 | 8 | None | Complete | CompatibilityOnly | Compatibility projection | `Terraria.Dome.Protocol.Compatibility.Verification` raw 11-byte frame | `v1456-compatibility-rerun-20260817-153000`, S2C sequence 1354 |
| 129 | FinishedConnectingToServer | S2C Server emit | `NetMessage.cs` case 129 | 0 | None | Complete | CompatibilityOnly | Protocol completion marker | Existing verifier | Not captured |
| 136 | CavernMonsterTypes | S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 136 | 12 | None | Complete | CompatibilityOnly | Compatibility projection | `Terraria.Dome.Protocol.Compatibility.Verification` raw 15-byte frame | `v1456-compatibility-rerun-20260817-153000`, S2C sequence 1355 |
| 138 | ClientSyncedInventory | C2S Dispatcher | `Main.cs` `TrySyncingMyPlayer` -> `NetMessage.SendData(138)`; no `MessageBuffer` switch case | 0 | None | Complete | CompatibilityOnly | Empty notification is validated after activation, then explicitly ignored; source sends it after one or more inventory packets. | `Terraria.Dome.Protocol.Compatibility.Verification` raw 3-byte frame | Not captured |
| 139 | HostStatus | S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 139 | 2 | None | Complete | CompatibilityOnly | Compatibility projection | `Terraria.Dome.Protocol.Compatibility.Verification` raw 5-byte frame | `v1456-compatibility-rerun-20260817-153000`, S2C sequence 1361 |
| 147 | SyncLoadout | C2S Dispatcher / S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 147 | 4 | None | Complete | CompatibilityOnly | Bootstrap parser | `Terraria.Dome.Protocol.Compatibility.Verification` raw 7-byte frame | `v1456-compatibility-rerun-20260817-153000`, C2S sequence 11 |
| 154 | Ping | C2S Dispatcher / S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 154 | 0 | None | Complete | CompatibilityOnly | Controlled response | Existing verifier | Not captured |
| 155 | SyncChestSize | S2C Server emit | `MessageBuffer.cs` / `NetMessage.cs` case 155 | 4 | None | Complete | CompatibilityOnly | Compatibility projection | `Terraria.Dome.Protocol.Compatibility.Verification` raw 7-byte frame | `v1456-compatibility-rerun-20260817-153000`, S2C sequence 368+ |
| 159 | RequestSection | C2S Dispatcher | `MessageBuffer.cs` case 159 | 4 | None | Complete | CompatibilityOnly | Request projection | Existing verifier | Not captured |

## PlayerControls Source Contract

The V1456 source reads the fixed state in `MessageBuffer.cs` case 13 and writes the same
order in `NetMessage.cs` case 13. Payload length, after the message ID, is:

```text
14
+ (flags2.bit2 ? 8 : 0)
+ (flags2.bit7 ? 2 : 0)
+ (flags3.bit6 ? 16 : 0)
+ (flags4.bit5 ? 8 : 0)
```

The initial raw mount regression failed before the decoder work with
`Terraria PlayerControls packet has unsupported optional fields.` The focused verifier now exits
`0` after checking exact 14/22/16/30/22-byte individual forms, the 48-byte maximum form, and
mount/return/camera truncation boundaries. `Terraria.Dome.SessionReplication.Verification` also
exits `0` with a mount frame followed by Ping, proving the legal frame does not close the session.

The corresponding outbound state is not currently sent to the source session after activation;
the server's replication path excludes self-replication. Incoming mount data remains
session-local until an independently verified Simulation authority migration exists. It is not
echoed to observers.

## Executable Evidence

On 2026-08-17, the rebuilt
`Terraria.Dome.Server` exited `0` with `0` warnings and `0` errors. Its artifact was
`Build/bin/Terraria.Dome.Server/Release/net10.0/Terraria.Dome.Server.exe`; the restarted
process was verified listening on `127.0.0.1:7778`.

The focused protocol verifier, session replication verifier, hardening loopback verifier,
full-client bootstrap verifier, and `python -m unittest
Build/diagnostics/test_compare_packet_traces.py -v` all exited `0`. A fresh source-built
client `join-stable` trace is under
`Build/diagnostics/v1456-compatibility-20260817-140623`:

- `client/join-stable.json`: success after 24,280 ms, player slot 1, `netMode` 1.
- `summary.md`: 1,557 complete frames and 0 invalid frames.
- `comparison.md`: the normal and Dome C2S message IDs, counts, and length ranges match;
  all 20 observed S2C message-family counts match. The remaining S2C length differences are
  explicitly classified as authoritative world-name, compression, tile optional-state, or NPC
  optional-state content.

The source client automation also exposes `mount-stable`. The authorized runtime configures
`ItemID.SlimySaddle`, invokes `Player.QuickMount()` after active join, requires mount type `3`,
and waits at least 1,000 ms while the connection, local player, and mount remain active. The
result at
`Build/diagnostics/v1456-compatibility-20260817-191100-quicklaunch/client/mount-stable.json`
reports success for player slot 2 after 7,854 ms.

The trace at
`Build/diagnostics/v1456-compatibility-20260817-191100-quicklaunch/trace.jsonl` contains 1,495
complete frames and zero invalid frames. Its sole C2S `PlayerControls(13)` frame is sequence
1402: the 19-byte frame has a 16-byte payload, exactly matching
`14 + (flags2.bit7 ? 2 : 0)`; `flags2` is `0x90` and its selected `UInt16` mount type is `3`.
`summary.md` records the frame, and `comparison.md` classifies it as the expected Dome-only
input relative to the normal `join-stable` baseline, which intentionally did not mount.

This evidence proves complete wire consumption and session liveness for a real mount-bearing
client frame. Mount physics, collision, persistence, and observer replication remain outside
Simulation authority; the server retains incoming mount state only as session-local compatibility
state and does not echo it to observers.
