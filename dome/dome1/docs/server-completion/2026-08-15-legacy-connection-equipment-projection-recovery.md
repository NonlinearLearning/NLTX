# Legacy Connection Equipment Projection Recovery

Date: 2026-08-15

## Symptom

A complete Terraria client could enter a Dome world after the earlier initial-section recovery,
but it received only a partial initial state and disconnected shortly afterward. The historical
client trace also showed normal active packets immediately after entry, so draining a synthetic
initial stream before sending active traffic was insufficient coverage.

## Reference Evidence

`D:\TRbackup\Version4物理删除了某些文件\Terraria\NetMessage.cs:2543-2553` calls
`SyncOnePlayer` from `SyncConnectedPlayer`. Its connection path sends message `5`
(`SyncEquipment`) only for the player-visible connection equipment groups:

| Slot range | Source group | Frames |
| --- | --- | ---: |
| `0..58` | inventory and mouse item | 59 |
| `59..78` | armor | 20 |
| `79..88` | dyes | 10 |
| `89..93` | miscellaneous equipment | 5 |
| `94..98` | miscellaneous dyes | 5 |
| `900..929` | loadout 1 armor and dyes | 30 |
| `930..959` | loadout 2 armor and dyes | 30 |
| `960..989` | loadout 3 armor and dyes | 30 |

The total is `189` frames. The `SyncOnePlayer` calls are visible at
`NetMessage.cs:2665-2677`; each array group is transmitted by
`SyncOnePlayer_ItemArray` at `NetMessage.cs:2712-2719`.

`D:\TRbackup\无任何删减通过编译\Terraria.ID\PlayerItemSlotID.cs:183-200`
establishes the underlying allocations. It separately allocates banks, trash and other account
slots between the base slots and loadouts. Those slots are not part of the `SyncConnectedPlayer`
connection projection.

## Root Causes

`PlayerPersistentState` correctly retains all `990` account slots. Before this repair,
`PlayerPersistentStateMapper.ToAuthorityFrames` also emitted every persisted slot when a player
entered the world. That produced `990` `SyncEquipment` frames instead of the reference
connection projection's `189`, including bank and other non-connection state.

The new concurrent replay also proved a second transport defect. The initial projection used the
background `SessionReplicationState` FIFO, while dispatcher responses and `WorldData` were
written directly by `TerrariaProtocolSessionHost`. A client `Ping` sent immediately after
`PlayerSpawn` was therefore returned before `FinishedConnectingToServer`, interleaving an active
response into the initial completion projection.

## Repair

`src/Terraria.Dome.Server/Protocol/PlayerPersistentStateMapper.cs` now projects only the exact
connection-visible union:

```text
0..98 and 900..989
```

The mapper retains all `990` slots in the persistent account; this change is outbound
connection projection only. It also has an explicit `0..989` bound so malformed persisted slot
identities cannot enter the connection stream.

`src/Terraria.Dome.Server/Protocol/TerrariaProtocolSessionHost.cs` now queues post-handshake
dispatcher response frames and `WorldData` through the existing per-session
`SessionReplicationState` writer. The SetUserSlot hello response remains before that writer is
created. After that boundary every server-to-client frame is serialized by one FIFO writer, so
the initial completion projection remains contiguous and `FinishedConnectingToServer` closes it.

## Regression Evidence

The account-authority verifier was first changed to require the reference slot set. Before the
mapper repair it exited `1` with:

```text
The server did not emit a complete player authority stream.
```

After adding the concurrent full-client replay, the pre-serialization host exited `1` with:

```text
Full-client completion must be PlayerSpawn -> PlayerActive -> PlayerProfile ->
PlayerControls -> authority state -> FinishedConnectingToServer without interleaved active responses.
```

The replay sends `SyncPlayerZone` (`36`), `PlayerBuffs` (`50`), `PlayerControls` (`13`),
`SyncProjectile` (`27`) and `Ping` (`154`) immediately after `PlayerSpawn`, while the client
reads the queued initial authority stream. It requires the exact `189` equipment-slot sequence,
no active response before completion, the queued Ping acknowledgement, and a later valid active
connection.

The following commands were run from the repository root on 2026-08-15. All exited `0`:

```powershell
dotnet run --project Test\Terraria.Dome.PlayerAuthority.Verification\Terraria.Dome.PlayerAuthority.Verification.csproj -p:UseSharedCompilation=false --no-restore
dotnet run --project Test\Terraria.Dome.FullClientBootstrap.Verification\Terraria.Dome.FullClientBootstrap.Verification.csproj -p:UseSharedCompilation=false --no-restore
dotnet run --project Test\Terraria.Dome.Combat.Protocol.Verification\Terraria.Dome.Combat.Protocol.Verification.csproj -p:UseSharedCompilation=false --no-restore
dotnet run --project Test\Terraria.Dome.Hardening.Loopback.Verification\Terraria.Dome.Hardening.Loopback.Verification.csproj -p:UseSharedCompilation=false --no-restore
dotnet build Terraria.Dome.sln -p:UseSharedCompilation=false -m:1
```

The final solution build restored no new projects and completed with `0` warnings and `0` errors.
All artifacts were produced under the repository's `Build/` output policy.

## Residual Scope

This is protocol-loopback evidence, including concurrent active traffic, not a claim that a new
GUI Terraria client session was captured after this build. The existing `7777` proxy log remains
historical evidence until a GUI reconnect creates a fresh trace.

The repair does not make client-supplied equipment, life, mana or projectiles authoritative.
Active compatibility packets remain ownership- and wire-format-validated without mutating the
server-owned persistence or simulation state. Bank, chest and other account synchronization are
separate intentional protocol paths and are not emitted as part of initial player connection.
