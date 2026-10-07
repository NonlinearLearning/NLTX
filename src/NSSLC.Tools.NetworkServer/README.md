# NLTX world-data network host

This executable is a narrow, headless network host for validating Terraria login, world-section
transfer, and player synchronization. It uses the production gateway, a bounded player-slot
authority, and the repository's WorldFile decoder. Packet 6 is answered with packet 7 projected
from the loaded world document; the server does not construct placeholder world metadata.

The [verification behavior classification](../../Test/NSSLC.Infrastructure.Network.Verification/README.md#行为分类与验收边界)
separates protocol/session checks, partial terrain behavior, and complete gameplay acceptance.
The current mining, placement and projectile probes do not accept the complete gameplay flows:
mining drops, pickup/inventory transfer, placement consumption and combat outcomes remain outside
their assertions. A passing network-probe group must be reported with that narrower scope.

Build from the repository root with the SDK in `global.json`:

```powershell
dotnet build src/NSSLC.Tools.NetworkServer/NSSLC.Tools.NetworkServer.csproj
dotnet Build/bin/NSSLC.Tools.NetworkServer/Debug/net10.0/NSSLC.Tools.NetworkServer.dll `
  --world 'D:/Worlds/world.wld' --listen 127.0.0.1 --port 7777
```

Set `NLTX_SERVER_PASSWORD` in the environment to require a password. The host binds only to
loopback by default. Use Ctrl+C for orderly shutdown. `--exit-after-first-client` is intended for
automated probes and stops after the first admitted client disconnects.

Add `--ignore-client-version` to skip the exact `Terraria319` Hello-version comparison for
cross-version testing, including a Steam 1.4.5.8 client sending `Terraria326`. Password admission,
player-slot allocation, packet decoding, and handler policies still use their normal paths.
This option does not translate packet layouts between versions. The startup log identifies the
selected version-check mode, and session failures include the gateway code and packet ID.

For the Steam 1.4.5.8 client, also add `--steam-module-ids`. Its packet 82 registry includes
CreativeUnlocks at module 5, moves CreativePowers to module 6, and shifts the following modules
through CraftingRequests. The host selects the matching module codecs and resolves admission
policies using the repository's logical module IDs. The module layout is selected for the whole
host; it is not negotiated separately for each connection. The startup log reports the layout,
and gateway diagnostics include the wire module ID, action when available, and body length.
The Steam mode also selects the confirmed packet 27/29 projectile layouts: packet 27 starts
with a packed 32-bit key (spawner, index, generation), and packet 29 contains that key and a
position. The profile keeps the generation bits, reuses the existing optional projectile-state
codec, and validates the packet body before admitting the upload. Legacy projectile bindings
remain selected when Steam mode is disabled.

```powershell
dotnet Build/bin/NSSLC.Tools.NetworkServer/Debug/net10.0/NSSLC.Tools.NetworkServer.dll `
  --world 'D:/Worlds/world.wld' --listen 127.0.0.1 --port 7777 `
  --ignore-client-version --steam-module-ids
```

Enabled messages are limited to the login handshake, RequestWorldData (6), initial section request
(8), tile-section transfer (10), player spawn (12), player controls (13), test-only tile
manipulation (17), extra section request (159), and the required stage confirmations (9, 49, 129).
Packet 82 also supports map Ping and the personal spawn-rate slider (CreativePowers power 14).
The enabled gateway Ping capability also echoes the empty latency packet (154) back to the
same Active connection. Steam sends this packet about every 250 milliseconds while waiting
for each response; the gateway admits up to five requests per one-second rate window.
Payload bytes, pre-admission requests, and excessive request rates remain rejected.
The slider handler validates a finite value in `[0, 1]` and records it against the authenticated
player slot for the headless probe report. It does not change NPC spawning or persist the value;
other CreativePowers operations remain closed.
After spawning, the host accepts team synchronization (45), zone synchronization (36), and
conversation-state reports (40), with replies bound to the authenticated player slot. Team
values are limited to 0 through 5, and conversation indices to the vanilla NPC slot table or
the no-conversation value `-1`. The player uploads already accepted during login (4, 5, 16,
42, 50, 147) also remain available in the Active stage for appearance, inventory, life, mana,
buff, and loadout updates. All of these use explicit size/rate policies and the existing
validation paths. They are headless synchronization probes; conversation replies do not run
NPC interactions, and the reported state does not drive authoritative combat or simulation.
In Steam mode, Active sessions may upload projectile state (27) and termination (29). The
headless handlers require the packed key's spawner to match the authenticated player slot
and record bounded per-player counts, the last key, and projectile type in `ProjectilePackets`
in the shutdown report. These handlers observe synchronization; they do not create runtime
projectiles, run AI, apply damage, or broadcast projectile state to other clients.
Player-control packets are validated and recorded for the headless probe; the host does not
simulate authoritative movement or collision. Each accepted position also sends missing terrain
in the surrounding 3 by 3 section neighborhood, as the Steam server does during movement.
The transfer history includes initial sections and explicit requests, belongs to the bound
player's current connection identity, and resets on slot reuse or reconnection. Repeated controls
do not resend loaded sections; explicit packet 159 requests still refresh a section. The same
tile-section cache and tile-change invalidation are used by all three transfer paths.
Its packet 17 handler accepts a nearby,
in-bounds break request for an active non-frame-important tile. Partial mining hits (action 0,
fail flag 1) leave the tile intact. A final hit changes an in-memory overlay, invalidates the
affected section cache, and sends the authoritative tile state in packet 20. Repeated hits on
an already empty tile return its current state; framed tiles beyond this probe's supported
break behavior are restored rather than closing the connection.
The Active-stage mining notice (125) validates world coordinates without changing terrain;
its last byte carries pick damage despite the existing DTO field name `TileType`. Tool-use
sound notices (152) are also admitted without a self echo or sound relay in the headless host.
Weapon rotation and animation notices (41), used by some shooting styles, are admitted only
in Active, with finite rotations and nonnegative animation timers. The headless host does
not simulate or relay weapon animations and does not echo the notice to the sender.
Packet 41 allows up to 120 requests and 840 body bytes per one-second window.
Packets 17, 125 and 152 allow up to 120 requests per one-second window. Buff synchronization
(50) allows the same count with a 12 KB byte budget, preserving the 44-buff per-packet bound.
The probe requests that section again to verify the cached section reflects the break.
Packet 17 action 1 also supports nearby ordinary single-cell block placement into an empty
cell. It preserves the existing wall, wires and liquid, invalidates the section cache, and
returns the resulting cell in packet 20. Repeated placement into an occupied cell and
placement of frame-important objects return the stored cell without closing the connection.
The live placement verifier checks twelve placements, occupied-cell corrections, a fresh
section containing those blocks, and latency Ping replies after placement. Furniture framing,
support rules, inventory consumption and persistent saving are not implemented by this overlay.
The probe does not exercise drops, tile framing, protection rules, or persistence, and never writes
the source `.wld` file. Other gameplay packets remain closed. The AI system is not implemented and
this host does not run NPC or projectile AI. Passing the probe does not mean the host supports full
gameplay.
