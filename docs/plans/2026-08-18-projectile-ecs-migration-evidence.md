# Projectile ECS Migration Evidence

Status: **partial**. This evidence accepts the typed `linear` and `gravity` projectile slice;
it does not claim full legacy `Projectile.cs` parity or authorize deleting the legacy source.

## Legacy inventory

The read-only Roslyn inventory was regenerated from
`D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs`.

- SHA-256: `97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B`
- 54,799 lines and 1,442,289 bytes
- 1 `SetDefaults`, 91 `NewProjectile` invocation nodes, and one each of `Update`, `AI`,
  `Damage`, and `Kill`
- 790 `aiStyle` assignments and 206 distinct values
- 6,203 `Main`, 90 `NPC`, 65 `Player`, 704 sound/audio, 177 `netUpdate`, 595 `timeLeft`,
  and 724 `penetrate` legacy syntax tokens

The generated artifact is
`Build/diagnostics/projectile-migration-20260818/behavior-index-generated.json`. The coverage
manifest is `behavior-index.json`: only `linear` and `gravity` are migrated; every other legacy
behavior is explicitly unsupported and cannot silently fall back to the old type.

## Runtime evidence

All commands were run from the repository root in Release configuration with
`-p:UseSharedCompilation=false`.

| Gate | Result |
| --- | --- |
| Simulation rebuild | Exit 0, 0 warnings, 0 errors; DLL in `Build/bin/Terraria.Dome.Simulation/Release/net10.0/` |
| Server rebuild | Exit 0, 0 warnings, 0 errors; DLL in `Build/bin/Terraria.Dome.Server/Release/net10.0/` |
| Combat verifier | Exit 0: bounded ordered damage, collision, cooldown, deterministic loot |
| Combat protocol verifier | Exit 0: V1456 combat projection and PVS revision cursors |
| Completion verifier | Exit 0: manifest 100 points, core-server evidence 92 points |
| Combat loopback | Exit 0 twice: PVS-limited combat and exactly one server-owned drop |
| Hardening loopback | Exit 0: malformed peer, flood, disconnect, slow reader and rapid reconnect isolation |
| Full-client bootstrap | Exit 0: no source-player authority replay |

The command-by-command exit statuses are in
`Build/diagnostics/projectile-migration-20260818/verification.json`.

## Protocol and boundary evidence

`Test/Terraria.Dome.Combat.Protocol.Verification` verifies V1456 message 27 through the typed
`ProjectileSyncPacket`: sparse `ai0` and `ai2`, banner, damage, knockback, original damage and
UUID survive exact encode/decode. It also verifies message 29's exact three-byte payload:
`06 00 1D 01 00 05` identifies projectile `1` owned by player `5`; a forged owner and trailing
payload byte are rejected.

The Simulation legacy-dependency scan had zero matches for `Terraria.Projectile`, `Main.`,
`NetMessage`, `SoundEngine`, `AI(`, and `Update(`. The accepted runtime path is therefore based
on typed components, systems, snapshots and protocol DTOs rather than the legacy object API.

## Residual scope

- Only the first two behavior families are covered; bounce, tracking, explosions, grapples,
  whips, minions, sentries, boss families and the remaining legacy `AI_###` surface are not
  migrated.
- The legacy source is unchanged and intentionally remains outside this change's deletion scope.
- The completed runtime gates demonstrate the selected ECS slice. They do not establish complete
  Terraria projectile compatibility.
