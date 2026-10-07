# NPC AI Guide attack launch and cycle checkpoint — 2026-10-07

## Source boundary

This follow-up batch closes the Guide-specific `ai[0] == 12` launch window from
the read-only source range `NPC.cs:55312-55582`.

Reference source:
`D:/TRbackup/无任何删减通过编译/Terraria/NPC.cs`

Reference SHA-256:
`ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0`

The extracted range is preserved in
`Build/diagnostics/NpcAiRedesign/guide-source-profile-20261007/source-attack-excerpt.txt`.
Source golden comparison is **not-run** because no source golden exists.

## Implemented behavior

`NpcGuideAttackConfigurationProfile` now returns and verifies:

- normal mode projectile `1`, base damage `12`, cooldown `30 + Next(20)`;
- hardmode projectile `2`, base damage `18`, cooldown `15 + Next(10)`;
- source speed `10f`, spawn tick `1`, knockback `2.75f`, aim offset `4`, and
  spread `0.7f`;
- integer Guide town-NPC damage scaling;
- attack timer decrement, `localAI[3]` increment, and frame reset at the source
  attack timer;
- target aim toward the caller-selected danger target, source horizontal-sign
  fallback, random X/Y spread, and spawn position
  `Center + (spriteDirection * 16, -2)`;
- `npcProj` and `noDropItem` launch flags;
- cycle-end `ai[0]` selection (`localAI[2] == 8 && flag16`), cooldown random
  consumption, `localAI[1] = localAI[3]`, and network update request.

The profile emits `NpcGuideProjectileSpawnRequest`; the existing projectile
owner executes it through `INpcGuideAttackEffectPort`. No projectile store,
network host, or entity registry was added or accessed.

## Ownership

| Concern | Owner |
| --- | --- |
| danger target selection (`num59`) | caller-provided selected-target snapshot |
| Guide attack calculation | `NpcGuideAttackConfigurationProfile` |
| spread and cycle random source | `INpcGuideAttackRandomPort` |
| projectile creation | existing projectile owner through `INpcGuideAttackEffectPort` |
| network update | existing network owner through the same effect port |
| task lifecycle | existing `NpcTaskLifecycleSystem` / `NpcTaskReference` |

The patch does not modify `RuntimeNpcEntity`, `RuntimeNpcStore`, Simulation
`Program`, `ReferenceVerification`, Blue, Mother, Eye, or canonical plans and
ledgers.

## Verifier and evidence

The independent verifier now covers:

1. normal and hardmode configuration;
2. target aim, spawn geometry, spread-port consumption, damage, and horizontal
   damping;
3. projectile effect ordering;
4. cycle cooldown randomization, preservation of `localAI[2]`, `ai[0] == 8`
   return selection, and network effect ordering;
5. all previously recorded Guide return-home, resting-spot, sitting,
   walk-prediction, conversation, danger, dialogue, idle, and shared-task
   scenarios.

Evidence directory:
`Build/diagnostics/NpcAiRedesign/guide-source-profile-20261007/`

- `build-npc-attack-cycle-final.txt`: NPC build exit `0`, zero warnings/errors;
- `build-guide-verifier-attack-cycle-final.txt`: verifier build exit `0`, zero warnings/errors;
- `run-guide-verifier-attack-cycle-final.txt`: verifier exit `0` with the Guide `PASS` line;
- `final-fingerprint.txt`: profile and binary hashes, `sourcegolden=not-run`.

## Remaining boundary

The source `ai[0] == 10` generic town-NPC attack branch has no Guide-specific
configuration arm in this range. Ordinary `ai[0] == 0/1` movement, door/gate
mutation, jumping, and the remaining town-NPC animation branches remain
caller-owned or open slices; this checkpoint does not claim those behaviors.
