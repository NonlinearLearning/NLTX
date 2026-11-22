# CR-2026-10-22: Spawn Slime and Guide for AI and Damage Validation

- ID: `CR-2026-10-22-NPC-AI-DAMAGE-TEST`
- Status: native and manual-server corrections checked; original-client visual acceptance pending; dropped-item pickup accepted
- Date: 2026-10-22
- Level: major
- Scheduled iteration: current task
- Execution strategy: `spec`, three implementation batches
- Approved by: user

## Original request

> 在世界中生成1个10w血的史莱姆和向导测试其ai和扣血正确性

## User decision

> **重大变更、当前任务实施、spec 分批执行**

This authorizes the major scope, the current task schedule, and formal spec batches.

## Acceptance correction (2026-10-22)

Original feedback:

> 攻击史莱姆致死会导致游戏卡顿,攻击史莱姆时会出现很多粒子特性

User-selected correction:

> 保持原版客户端，测试史莱姆改回原生生命

The current task now uses a native 25/25 blue slime and the ordinary Guide. The previous
100000-life runtime experiment remains historical evidence only. Do not modify the original
client or emulate maximum life through difficulty: the vanilla life multiplier caps at four
and changes damage/knockback too. Correct initial player spawn positioning and verify NPC
lifecycle/synchronization before requesting another original-client acceptance.

## Current evidence

- Production spawns are a native 25/25 blue slime and 250/250 Guide. Packet 23 has no arbitrary
  maximum-life field; the native target avoids the client's hit-particle denominator mismatch.
- The built-in NPC tick uses captured player and world facts; Packet 28 damage is routed to the
  combat owner with authenticated player and entity-generation checks.
- A vanilla Packet 12 personal spawn of -1/-1 resolves to world/team spawn before activation can
  drive NPC CheckActive. The previously misplaced player could retire the slime before movement.
- Current native reports pass 30 TCP and 12 AI checks. Original-client visual confirmation of
  slime hit particles and death stutter remains pending.

## Acceptance correction (2026-11-17): manual slime AI and immunity

User feedback:

> 将史莱姆设置为无敌
>
> 史莱姆依旧会的不正确行为加剧了参考D:\TRbackup\MinimalImplementation和D:\TRbackup\Version4修复

The user authorized this correction in the current task. Keep the original-client source and the
two reference trees read-only. The server definition also still had 50/50 life despite the prior
native-blue-slime decision and the reference's 25 life; restore 25/25 while keeping the immunity
capability. The manual calculator treated every usable player target as an
aggressive state. Both reference implementations gate aggression on nighttime, changed life,
position below the world-surface threshold, or slime rain. The corrected calculator uses that
predicate for the one-frame/two-frame grounded jump timer, damps horizontal speed while grounded,
and steers toward a target while airborne. Movement and Spatial still own velocity, position, and
collision commits. The server world clock now starts with the loaded world's day/night flag. The
current runtime does not model slime-rain state, so its manual world fact supplies `false` for that
condition. Project the manual slime timer/facing into the type-1 NPC packet so clients do not
receive zeroed AI fields on every update. Version4's client does not interpret the reserved
server-authority bit, so native client prediction remains a compatibility limitation unless a
client-side change is authorized.

Mark the spawned type-1 slime with damage immunity and reject it at the shared Combat request
boundary. Player hit packets and projectile hits both converge on this owner; leave Guide and other
NPC damage behavior unchanged. This explicitly supersedes the earlier acceptance that expected
the test slime's health to decrease.

Implementation and acceptance evidence are tracked in
`.agent-workplace/docs/task/2026-10-22-npc-ai-damage-test.md`.

Manual-server evidence: `NSSLC.Server` built with exit 0, 19 existing warnings from untouched
`NSSLC.WorldGeneration`; the NPC AI verification project built with exit 0, zero warnings; the
`--manual-ai` probe passed calm-ground, airborne-steering, aggressive timer/jump, collision,
grounding-stability, and immunity assertions. The same verifier also passed its shared-AI run.
The final server build and both verifier runs pass after restoring 25/25 life. The updated server
listens on `127.0.0.1:7777` with PID 19764 and logs `ai=manual`. Original-client visual behavior
remains unverified, and the client continues to run its own native AI because it ignores the custom
authority bit.

## Follow-up acceptance defect: dropped-item pickup

Original feedback:

> 某些物品捡不起来

> 被丢弃的物品不能捡起

A dropped unowned coin can make the all-item physical update unavailable while the dropping
player is excluded for 100 frames. Ownership timers previously committed in that same failed
transaction, so they could not expire. The correction prepares guarded timer-only mutations on
unavailable motion/collision ticks. Ordinary successful ticks keep their existing timer step;
unavailable motion remains visible, without invented NPC/collision facts or inventory credits.

The affected server build passed with 0 warnings and 0 errors. An independent no-save instance
started on `127.0.0.1:7791` (PID 30840); the original 7790 listener remained untouched. Real-client
logs show type 129 gray brick and type 9 wood drops assigned to the player after their delay and
retired on pickup. The user confirmed the coin/wood prompt with “两种物品都能重新捡起”; coin
pickup is user-reported, since no coin-drop event appears in the captured new session. Other
content/pickup cases and NPC visual acceptance have not been inferred from that result.

## Scope

Add a repeatable, isolated runtime validation path that starts with exactly one type-1 slime at
native 25/25 life and one ordinary type-22 Guide. Connect only the required NPC definitions,
server-authoritative AI tick inputs, client NPC synchronization, authenticated NPC-hit handling,
and resulting health/death broadcasts. Validate both NPCs' AI and the slime's damage behavior with
the supplied original client against an independent copy of the current generated world.

The test NPCs are runtime-only and must not be written into the source WorldFile. Use a fresh
world copy and an unused listener; do not stop, replace, or modify existing instances or their
worlds on ports 7780–7789.

## Acceptance criteria

1. The isolated runtime creates exactly one active type-1 slime and one active type-22 Guide.
2. The slime begins at exactly 25 current and maximum life, matching the original client's native
   blue-slime defaults. Packet 23 uses the native full-life representation without difficulty overrides.
3. The built-in authoritative tick advances slime AI using captured player and world collision
   facts; the Guide advances only the Guide/town AI behavior supported by the supplied client
   implementation. Both NPCs remain valid and synchronized through the tick.
4. A real client can enter the isolated instance, see both NPCs, and perform the supported slime
   AI behavior. Client damage is accepted once, applied authoritatively with the verified defense
   and critical-hit rules, and the resulting current life is synchronized back to the client.
5. Lethal damage clamps life to zero and retires/synchronizes the NPC exactly once. Stale entity
   generations, repeated packet delivery, invalid NPC slots, and unauthorized hits cannot damage a
   replacement entity.
6. An affected-project build and focused runtime checks pass after each batch. Any original-client
   result is recorded separately from domain-only probes.

## Implementation evidence

- Batch 1 built and validated the production spawn catalog, runtime-only custom slime maximum life,
  Guide defaults and Packet 23 Int32 health width.
- Batch 2 ran 600 built-in host AI frames against captured world collision and neutral-player facts;
  missing inputs and stale Guide revisions were rejected without state changes.
- Batch 3 built the server and ran 29 focused checks over real framed TCP sessions, including a two-
  client join, AI snapshots, ACK 162 ordering, normal/critical/leftward damage, invalid/stale slot
  handling, duplicate sequence rejection, observer Packet 28 broadcast before Packet 23 health,
  terminal-send retry and generation-safe retirement.
- Original client visual and weapon interaction remain pending. A no-save diagnostic instance is
  listening on `127.0.0.1:7790`, loading an independent copy at
  `Build/runtime/NpcAiDamage-Live-2026-10-22/world-copy.wld`.
- Current native correction: 30 TCP checks and 12 AI checks pass, including default-spawn position,
  25-life synchronization, damage/ACK ordering, terminal death and generation safety. The earlier
  100000-life results above are historical and do not define current acceptance.

## Verification artifact paths

- B1 runtime report: `Build/runtime/NpcAiDamage-B1-2026-10-22/report.json`.
- B2 AI report: `Build/runtime/NpcAiDamage-B1-2026-10-22/report-b2.json`.
- Historical final baseline TCP report: `Build/runtime/NpcAiDamage-B1-2026-10-22/report-b3-final-baseline.json`.
- Historical final server build log: `Build/runtime/npc-ai-damage-final-build.log` (29 warnings, 0 errors).
- Historical final baseline TCP log: `Build/runtime/npc-ai-damage-final-baseline-probe.log`.
- Current native TCP report: `Build/runtime/NpcAiDamage-B1-2026-10-22/report-native-network.json`.
- Current native AI report: `Build/runtime/NpcAiDamage-B1-2026-10-22/report-native-ai.json`.
- Native build logs: `Build/runtime/npc-ai-native-server-build.log` and `npc-ai-native-probe-build.log`.
- Current 7790 runtime logs: `Build/runtime/NpcAiDamage-Live-2026-10-22/native.stdout.log` and `native.stderr.log`.
- Pickup build: `Build/runtime/item-pickup-server-build-final.log` (0 warnings/errors, exit 0).
- Pickup client/runtime evidence: `Build/runtime/ItemPickup-Live-2026-10-22/server.stdout.log`.

## Impact assessment

```json
{
  "status": "scheduled",
  "change_id": "CR-2026-10-22-NPC-AI-DAMAGE-TEST",
  "level": "major",
  "impact": {
    "db_change": false,
    "refactor": true,
    "affected_features": [
      "NPC definitions and runtime spawn",
      "authoritative slime and Guide AI ticks",
      "Packet 28 NPC damage input",
      "Packet 23 NPC life synchronization"
    ]
  },
  "decision": "accepted",
  "approved_by": "user",
  "scheduled_iteration": "current task"
}
```

- Database and WorldFile schema: no change. Test NPCs are runtime-only.
- Code areas: minimal server NPC definitions, NPC lifecycle/spawn, tick input capture, Guide and
  slime AI, inbound/outbound NPC packet routing, and isolated runtime setup.
- Compatibility risk: life-width encoding, NPC type defaults, Guide AI requirements, hit validation,
  and client/server tick ownership must match the supplied 1.4.5.8 client implementation.
- Safety boundary: preserve the user's running processes and files. Use a new listener and
  independent WorldFile copy; disable persistence for this diagnostic run unless the user requests
  otherwise.
- Estimated execution: three cohesive batches with a build and focused acceptance gate per batch.

## Out of scope

- Full NPC content catalog, all town-NPC behaviors, shops, quests, and unrelated combat/projectile
  routes.
- Persisting the test NPCs or changing WorldFile 326 encoding.
- Changing network version negotiation or any existing server listener.

## Decision history

| Field | Decision |
| --- | --- |
| Level | Major |
| Decision | Accepted |
| Approved by | User |
| Schedule | Current task |
| Strategy | `spec`, three batches |

